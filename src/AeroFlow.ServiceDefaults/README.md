# AeroFlow.ServiceDefaults

Golden-path observability for AeroFlow .NET services ([ADR-0022](https://github.com/aeroflow-air/platform-handbook/blob/main/docs/decisions/0022-observability-on-the-golden-path.md)). It is the shared package promoted from `AeroFlow.Local.ServiceDefaults` in aeroflow-local.

Version **0.1.0**. This package currently lives in `template-dotnet-service` because the organisation has no `lib-*` repository and no NuGet feed yet. The intended long-term home is a new `lib-service-defaults` repo that publishes this package (and `AeroFlow.ServiceDefaults.Testing`) to GitHub Packages. Until then, consume it as a project reference.

## What it provides

Call `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()`:

- OpenTelemetry traces, metrics and logs, with required resource attributes (`service.name`, `service.version`, `deployment.environment`, `aeroflow.squad`)
- OTLP export when `OTEL_EXPORTER_OTLP_ENDPOINT` is set; Azure Monitor when `APPLICATIONINSIGHTS_CONNECTION_STRING` is set
- The Azure SDK activity source enabled, so Service Bus can carry `Diagnostic-Id`
- `/health` (readiness) and `/alive` (liveness) in every environment, excluded from traces and request logs
- HTTP resilience and service discovery (the same defaults as the Aspire ServiceDefaults project)
- **Standard logs from the package itself**: every HTTP request, every message published or consumed (via `IObservabilityLog`), every failure — all with the same fields. Teams only add business events.

Analyser defaults also flow to consumers: CA1848 and CA2254 as errors, and `System.Console` banned in favour of `ILogger` / `[LoggerMessage]`.

See [docs/service-defaults.md](../../docs/service-defaults.md) for the event catalogue, field names, configuration and a business-event example. The contract-test helper is `AeroFlow.ServiceDefaults.Testing`.
