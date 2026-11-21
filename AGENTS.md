# template-dotnet-service

Golden-path template for AeroFlow .NET 8 services. New `svc-*` repos are created from it
(skill `aeroflow-new-service-checklist`). `BookingsController`, `Models/` and `BookingLog` are placeholders.
When you create a service from it, rewrite this file for that service's domain and topics.

## Non-negotiables (* = enforced by build or CI)
- The HTTP edge is `[ApiController]` controllers in `Controllers/`, not minimal APIs.
- The domain is pure (ADR-0004, [coding standards](https://github.com/aeroflow-air/platform-handbook/blob/main/docs/standards/coding-standards.md)):
  static functions, immutable `sealed record`s, `Result<T>` with typed errors, `Option<T>` instead of `null`.
  No I/O, logging, clock or `throw` for business rules. The template has no `Domain/` yet, so copy svc-gate-allocation.
- Errors leave as ProblemDetails 400/404/409 with a `code` extension. Follow gate-allocation's `ProblemResults.cs`.
  The demo's inline `NotFound(new ProblemDetails ...)` is a placeholder.
- * `AddServiceDefaults()` + `MapDefaultEndpoints()` give OTel, request logs, `/health` (readiness) and `/alive`
  (liveness, `HealthEndpointTests`). Don't write your own HTTP or messaging log lines.
- * Log with `[LoggerMessage]` only (CA1848/CA2254), never `Console`/`Debug.WriteLine` (RS0030). Business event
  ids start at 10,000. Ids below that are the platform's. No PII, secrets or bodies in logs.
- Publish only to your own `<capability>-events` topic, via the outbox. Handlers are idempotent (ADR-0013/0017/0019).
- * `workload.yaml` (ADR-0009): set `workload`/`repo`, and add `queue` + `events:` when using Service Bus (`workload.yml`).
- Workflows pin `aeroflow-workflows` tags (never `@main`) and declare job permissions. This repo is on `v0.5.0`,
  plus `v0.7.0` for agent skills. Moving `dotnet-ci.yml` to `v0.6.0`+ needs `packages: read`.
- Update `docs/service.md` in the same PR when inputs, outputs, settings or failure modes change.

## ServiceDefaults here
`src/AeroFlow.ServiceDefaults*` is an in-repo copy of [lib-service-defaults](https://github.com/aeroflow-air/lib-service-defaults)
0.1.0, referenced by `ProjectReference`. `Directory.Build.props` imports its analyser props. Change the library
upstream, not here. The switch to the GitHub Packages feed is pending.

## Build and test
```bash
dotnet build --configuration Release && dotnet test   # xUnit, WebApplicationFactory, LogCollector
docker build -t aeroflow-service-template .
```

## Skills
Task skills are plain Markdown in `.claude/skills/<name>/SKILL.md`, pinned in `.agent-skills.yaml`. Any agent can
use them: read each `description`, follow the Steps, and open the Example links. Don't edit `aeroflow-*` folders
(* CI drift check). Change them in [agent-skills](https://github.com/aeroflow-air/agent-skills) instead.
