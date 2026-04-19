# Event Bus (Unity)

Sample Unity project demonstrating a **lightweight, type-safe publish/subscribe Event Bus** for decoupling game systems. The bus uses struct-backed events, explicit listener priorities, and dependency injection instead of a global static accessor.

## Documentation

Full design notes, API trade-offs, and usage examples:

**[Assets/Scripts/EventBus/README.md](Assets/Scripts/EventBus/README.md)**

## Highlights

- **Type-safe events** — `struct` payloads implementing `IEvent`; no string keys or reflection-based dispatch in the public API.
- **Ordered delivery** — subscribers run in a deterministic order via `EventPriority`.
- **Safe publish** — dispatch uses a snapshot so subscribing or unsubscribing during a callback does not corrupt iteration.
- **Explicit lifetime** — `EventSubscription` implements `IDisposable` for clear unsubscribe semantics.
- **DI-friendly** — register as a singleton (for example with VContainer) and inject `IEventBus` where needed.

## Requirements

- **Unity** 6 (`6000.0.x` — see `ProjectSettings/ProjectVersion.txt` for the exact editor version used in this repo).
- **[VContainer](https://github.com/hadashiA/VContainer)** — used for composition root and lifetime scopes (`ProjectLifetimeScope` registers the bus).

## Getting started

1. Clone the repository and open the project in the Unity Editor version listed above (or a compatible Unity 6 patch).
2. Let Unity import packages and regenerate the `Library` folder locally (do not commit `Library/` or `Temp/`).
3. Open the sample scene you use for development and press Play, or browse `Assets/Scripts/EventBus/` and the **[Event Bus README](Assets/Scripts/EventBus/README.md)** for API details.

## Repository layout

| Path | Purpose |
| --- | --- |
| `Assets/Scripts/EventBus/` | Event Bus implementation (`EventBus`, `IEventBus`, subscriptions, priorities) |
| `Assets/Scripts/Core/` | Lifetime scopes, sample UI/clicker code that exercises the project setup |
| `Assets/Scripts/EventBus/README.md` | In-depth Event Bus documentation |

## License

Add a `LICENSE` file at the repository root when you publish (this sample does not ship with one by default).
