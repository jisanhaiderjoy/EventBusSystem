# Event Bus

## Purpose

This Event Bus provides a lightweight, type-safe communication channel between systems without tight coupling.  
It is designed for Unity main-thread usage and favors predictable execution over framework complexity.

## Design Decisions

- Generic API constrained by `IEvent` and `struct` for strong typing and low allocations.
- No string keys in public API and no reflection-based dispatch.
- No static/global singleton usage. Register through DI and inject where needed.
- Listener execution order is deterministic through explicit priority sorting.
- Publish uses snapshot iteration (`ArrayPool` rent/return) to stay safe when listeners subscribe/unsubscribe during callbacks without a per-publish `ToArray()` allocation.
- Subscription lifecycle is explicit via `EventSubscription : IDisposable`.

## Trade-offs

| Decision | Benefit | Cost |
| --- | --- | --- |
| Snapshot iteration | Safe dispatch during list mutation | Transient buffer from `ArrayPool` |
| `readonly struct` events | Low GC pressure | Copy cost if event payload is too large |
| No static access | Better testability and composition | Requires DI wiring |

## Snapshot Dispatch

### The Problem

Iterating `_listeners` directly during `Publish` is unsafe. A listener callback is free to call `Subscribe` or `Dispose` (unsubscribe) on the bus while the loop is running. Both operations mutate `_listeners`, which can cause:

- `InvalidOperationException` — modifying a `List<T>` while enumerating it with a `foreach`.
- Skipped listeners — removing an element shifts indices, causing the loop to jump over the next entry.
- Double invocations — inserting a new listener mid-loop can put it in a position the counter has not yet reached.

### How It Works

```
ListenerCollection<T>.Publish(T eventData)
```

**Step 1 — Capture the count**  
`listenerCount` is read once before anything else. Any `Add` or `Remove` that happens inside a callback is invisible to the current dispatch round.

**Step 2 — Rent a buffer**  
```csharp
var snapshot = ArrayPool<EventListener<T>>.Shared.Rent(listenerCount);
```
`ArrayPool.Rent` returns a pre-allocated array from a shared pool. The returned array may be *larger* than requested, which is why `listenerCount` (not `snapshot.Length`) is used as the loop bound everywhere.

**Step 3 — Copy listener references**  
```csharp
for (var i = 0; i < listenerCount; i++)
    snapshot[i] = _listeners[i];
```
This is a cheap, index-based copy of the current listener set into the rented buffer. From this point forward the loop works exclusively off `snapshot`, so mutations to `_listeners` have no effect on the in-flight dispatch.

**Step 4 — Iterate and invoke**  
```csharp
var listener = snapshot[index];
if (!listener.IsActive) continue;
listener.Invoke(eventData);
```
Each entry is checked for `IsActive` before invoking. This handles the race between snapshotting and unsubscribing: `Unsubscribe` calls `listener.Deactivate()` *before* removing it from the list. If a listener is disposed mid-dispatch, the snapshot still holds a reference to it, but `IsActive` will be `false` and it will be silently skipped.

**Step 5 — Return the buffer**  
```csharp
finally { pool.Return(snapshot, clearArray: false); }
```
The `finally` block guarantees the buffer goes back to the pool even if a listener throws. `clearArray: false` skips zeroing the array on return. This is safe because:
- `EventListener<T>` holds no managed object references that need to be released for GC.
- The buffer is always fully overwritten in Step 3 before it is read in Step 4, so stale data from a previous rental can never be observed.
- Zeroing is unnecessary work for a transient scratch buffer.

### Why Not `ToArray()`?

`_listeners.ToArray()` would solve the mutation problem but allocates a new heap array on every single `Publish` call. In a game loop that publishes dozens of events per frame, that compounds into continuous GC pressure. `ArrayPool` eliminates that allocation in the steady state: the buffer is rented, used, and returned within the same call, with no heap allocation after warm-up.

---

## Usage

```csharp
// Registration (VContainer)
builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
```

```csharp
using Core.Events;

public sealed class CombatSystem
{
	private readonly EventBus _eventBus;

	public CombatSystem(EventBus eventBus)
	{
		_eventBus = eventBus;
	}

	public void OnPlayerKilled(int playerId)
	{
		_eventBus.Publish(new PlayerDiedEvent(playerId));
	}
}
```

```csharp
using Core.Events;

public sealed class HudPresenter : IDisposable
{
	private readonly EventSubscription _playerDiedSubscription;

	public HudPresenter(EventBus eventBus)
	{
		_playerDiedSubscription = eventBus.Subscribe<PlayerDiedEvent>(
			OnPlayerDied,
			EventPriority.High);
	}

	private void OnPlayerDied(PlayerDiedEvent evt)
	{
		// Update UI
	}

	public void Dispose()
	{
		_playerDiedSubscription.Dispose();
	}
}
```

## Future Extensions

- Async dispatch pipeline (`Task`-based listeners)
- Optional thread-safe mode with locking or queue handoff
- Event batching/coalescing
- Listener filtering predicates
