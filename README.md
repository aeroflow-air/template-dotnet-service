# AeroFlow .NET service template

Golden-path ASP.NET Core Web API template for the [AeroFlow Air](https://github.com/aeroflow-air) portfolio. Squads of roughly six use this to spin up `svc-*` repositories with a thin, containerised starting point — health checks, structured logging, ProblemDetails, and OpenTelemetry.

The shared observability package is `AeroFlow.ServiceDefaults` in this repository ([docs/service-defaults.md](docs/service-defaults.md), [ADR-0022](https://github.com/aeroflow-air/platform-handbook/blob/main/docs/decisions/0022-observability-on-the-golden-path.md)). The template service calls `AddServiceDefaults()` and `MapDefaultEndpoints()`. Until a `lib-service-defaults` repo and GitHub Packages feed exist, consume the package as a project reference.

## Create a new service from this template

**Preferred:** on GitHub, open this repository and choose **Use this template** → create `svc-booking` (or similar) under `aeroflow-air`.

**Or clone and rename:**

```bash
git clone https://github.com/aeroflow-air/template-dotnet-service.git svc-booking
cd svc-booking
# Rename solution, project folders, namespaces, and Docker image tags to match the service
```

Keep the layout: `src/`, `tests/`, `Dockerfile`, `.github/workflows/ci.yml`, and `.github/workflows/workload.yml`. Rename `AeroFlow.ServiceTemplate` to your service name throughout, and set `workload` and `repo` in `workload.yaml` to the new service name (for example `svc-booking`).

## Local run

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet restore
dotnet run --project src/AeroFlow.ServiceTemplate
```

- API: `http://localhost:8080` (or the port shown in the console)
- Health: `GET /health` (readiness) and `GET /alive` (liveness)
- Demo booking probe: `GET /api/bookings/ping` and `GET /api/bookings/{id}` (unknown ids return ProblemDetails)

```bash
dotnet test
```

## Container

```bash
docker build -t aeroflow-service-template .
docker run --rm -p 8080:8080 aeroflow-service-template
```

Then `curl http://localhost:8080/health`.

## What is deliberately not included

- **No Kubernetes** manifests or Helm charts
- **No Dagger** pipelines
- **No Pulumi** (or other IaC frameworks in-repo yet)
- **No heavy shared framework.** `AeroFlow.ServiceDefaults` is a small, independently versioned library for observability (ADR-0022). A squad can delete it and still ship, provided it still meets the signal and context-propagation contract.

Infrastructure as Bicep will land under [`infra/`](infra/README.md) later, using only governed platform modules (`br/platform:*`); see [ADR-0006](https://github.com/aeroflow-air/platform-handbook/blob/main/docs/decisions/0006-governed-bicep-modules-only.md). Platform conventions live in the **platform-handbook**; reusable Actions come from **aeroflow-workflows**.

## CI

`.github/workflows/ci.yml` calls the reusable workflow in `aeroflow-workflows` (`dotnet-ci.yml`, pinned to `@v0.5.0`), which restores, builds, and tests on pushes to `main` and on pull requests. `.github/workflows/workload.yml` calls `workload-deploy.yml` at the same pin: it validates `workload.yaml`, generates Bicep, and runs PSRule for Azure. The workflow file itself has no service name in it, so a newly scaffolded service only needs to rename the `workload` and `repo` fields in `workload.yaml`. Job-level `permissions` are explicit because the organisation `GITHUB_TOKEN` is read-only by default.

## Pointers

| Resource | Purpose |
| --- | --- |
| platform-handbook | Portfolio standards, CLAUDE.md constraints, ADR process |
| aeroflow-workflows | Shared GitHub Actions (dotnet-ci and decisions validation) |
| `infra/` | Placeholder for Bicep (governed `br/platform:*` modules only, see [ADR-0006](https://github.com/aeroflow-air/platform-handbook/blob/main/docs/decisions/0006-governed-bicep-modules-only.md)) — see `infra/README.md` |
| [docs/service-defaults.md](docs/service-defaults.md) | Observability package: event catalogue, log fields, analyser rules, contract-test helper |

## Licence / ownership

Internal AeroFlow Air template. Public repository; treat as the default starting point for new .NET services unless an ADR says otherwise.
