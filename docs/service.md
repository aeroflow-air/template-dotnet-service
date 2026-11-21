<!--
Service doc template (platform-handbook docs/standards/service-docs.md).
Copy to docs/service.md in the service repo. Keep every heading; write "None"
rather than deleting a section, so a reader can tell "nothing" from "forgot".
Describe what the code does today, not what is planned. Link the code.
-->

# <service-name>

> One sentence: what this service is responsible for, in business terms.

| | |
| --- | --- |
| Repo | `aeroflow-air/<repo>` |
| Owner | <squad> |
| Status | <prototype / local-only / deployed> |
| Runs in | <aeroflow-local AppHost resource name, hosting target> |
| Last reviewed | <YYYY-MM-DD> |

## Purpose

What problem it solves, what it owns (its data and its topic), and what it
deliberately does **not** do.

## Design decisions

Service-level choices and the reason for each. Link platform ADRs this service
follows; record anything service-specific here (or in an ADR if it binds other
services).

| Decision | Why | Link |
| --- | --- | --- |
| Observability is on by default via `AeroFlow.ServiceDefaults` | Golden-path traces, metrics, health and standard logs, without each service wiring exporters | [ADR-0022](https://github.com/aeroflow-air/platform-handbook/blob/main/docs/decisions/0022-observability-on-the-golden-path.md), [docs/service-defaults.md](service-defaults.md) |

## Architecture

Main components (domain, messaging, storage, API) and how a request or event
flows through them. A small Mermaid diagram is welcome.

## Inputs

### HTTP API

| Method | Route | Purpose | Responses |
| --- | --- | --- | --- |
| | | | |

Where the OpenAPI document and Scalar UI live, and how errors are shaped.

### Events consumed

| Topic | Subscription | Event types handled | Effect | Ignored |
| --- | --- | --- | --- | --- |
| | | | | |

Contract source (shared package or local copy, and which upstream type).

### Configuration

| Setting | Default | Meaning |
| --- | --- | --- |
| `Observability:ServiceName` | Host application name | `service.name` on every signal |
| `Observability:ServiceVersion` | Entry assembly informational version | `service.version` |
| `Observability:Environment` | Host environment name | `deployment.environment` |
| `Observability:Squad` | `unknown` | `aeroflow.squad` — set this |

## Outputs

### Events published

| Topic | Event type | When | Payload fields |
| --- | --- | --- | --- |
| | | | |

Message conventions (`MessageId`, `Subject`, `CorrelationId`, content type).
Write "None" if the service publishes nothing.

### Other side effects

State it owns, files, calls to other services.

## Dependencies

| Dependency | Needed for | Behaviour when absent |
| --- | --- | --- |
| | | |

## Failure modes

How the service fails and how you would notice. One row per failure.

| Failure | How it shows up | Impact | What to do |
| --- | --- | --- | --- |
| | | | |

Delivery semantics: at-least-once? idempotent? what is dead-lettered and why?

## Running and testing

- Standalone: `dotnet run --project src/AeroFlow.ServiceTemplate`. Hit `GET /health`, `GET /alive`, `GET /api/bookings/ping`.
- With the airport: not wired into aeroflow-local (this is the template, not a live service).
- Tests: `dotnet test`. Includes a contract test that asserts platform request logs carry the required fields.
- Healthy logs: an `AeroFlow.ServiceDefaults` event 1000 for each API request, plus business event 10000 on `/api/bookings/ping`. Health probes are not request-logged.

## Known gaps

Things that are knowingly missing or wrong, each with an issue link.
