# template-dotnet-service

Golden-path template for AeroFlow .NET services. New services are created from it, so it must stay the exemplar of the conventions below.

## Conventions (read before changing code)

This repo follows [ADR-0004: functional-style C#](https://github.com/aeroflow-air/platform-handbook/blob/main/docs/decisions/0004-functional-style-csharp.md).
The checklist is [C# coding standards](https://github.com/aeroflow-air/platform-handbook/blob/main/docs/standards/coding-standards.md). In short:

- Domain logic is **pure static functions** composed with `Bind` / `Map` / `Match` (like `GatePlanner.Apply(plan, command)` in svc-gate-allocation).
- Domain types are **immutable `sealed record`s** with immutable collections; change means returning a new value.
- States are **closed sum types**, ids are **value types** built via `Parse` / `Create`, never raw strings.
- Business failures return **`Result<T>` with typed error records** from `Domain/Errors.cs`
  (`ValidationError` / `NotFoundError` / `RuleViolation` → 400 / 404 / 409). No exceptions for business rules or control flow;
  `throw` only for bugs (`UnreachableException`) or invalid startup data.
- Absence is **`Option<T>`**, not `null`. Nullable types only in HTTP request/response records and wire messages.
- **No side effects in the domain**: no I/O, logging, `Console`, messaging, storage, static mutable state or
  `DateTimeOffset.UtcNow`. Time comes from `TimeProvider` at the edge.
- BCL only: the local `Result` / `Option` are deliberately small. No LanguageExt.
- Edges: async all the way (no `.Result` / `.Wait()`), flow `CancellationToken`, `[LoggerMessage]` structured logging.

## Where things live

- The demo code (`BookingsController`, `Models/`) is a placeholder. Real services put their pure core in `src/<Project>/Domain` (static planner/lifecycle functions, records, `Errors.cs`, small local `Result` / `Option`), with `Messaging/`, `Storage/` and `Controllers/` as the edges. See `svc-gate-allocation` for the reference shape.

## Events

- A service publishes only to **its own topic** (named `<capability>-events`) and only subscribes to others'. See ADR-0013.
- Handlers must be **idempotent**: Service Bus can redeliver, so applying the same event twice must be safe. Unknown, late or out-of-order events return an `Ignored` / `Rejected` outcome with a reason; they are not thrown (that sends them to the dead-letter queue).

## Tests and docs

- New domain functions and handlers need unit tests in `tests/`, including failure and duplicate-delivery paths.
  Domain tests need no mocks.
- Keep `docs/service.md` current: update it in the same PR when inputs, outputs, settings or known failure
  modes (with log signatures) change.
- When you rename the template for a new service, update this file to describe that service's domain and topics.
