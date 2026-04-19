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

## Usage

```csharp
// Registration (VContainer)
builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
```

## Namespaces

- Core types (`EventBus`, `IEventBus`, `IEvent`, `EventSubscription`, `EventPriority`, `EventListener`): `Core.Events`
- Example events (`PlayerDiedEvent`): `Core.Events`

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
