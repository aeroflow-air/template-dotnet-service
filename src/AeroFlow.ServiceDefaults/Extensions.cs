using System.Reflection;
using AeroFlow.ServiceDefaults;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Golden-path defaults: OpenTelemetry, health checks, HTTP resilience and
/// platform request/message logs (ADR-0022). Promoted from
/// <c>AeroFlow.Local.ServiceDefaults</c> in aeroflow-local.
/// </summary>
public static class AeroFlowServiceDefaultsExtensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

        var section = builder.Configuration.GetSection(ObservabilityOptions.SectionName);
        var options = new ObservabilityOptions
        {
            ServiceName = section[nameof(ObservabilityOptions.ServiceName)],
            ServiceVersion = section[nameof(ObservabilityOptions.ServiceVersion)],
            Environment = section[nameof(ObservabilityOptions.Environment)],
            Squad = section[nameof(ObservabilityOptions.Squad)],
        };
        var resource = ObservabilityResource.Resolve(
            builder.Environment,
            builder.Configuration,
            options,
            Assembly.GetEntryAssembly());

        builder.Services.AddSingleton(resource);
        builder.Services.AddSingleton<IObservabilityLog, ObservabilityLog>();
        builder.Services.AddTransient<IStartupFilter, ObservabilityStartupFilter>();
        builder.Services.AddOptions<ObservabilityOptions>()
            .BindConfiguration(ObservabilityOptions.SectionName);

        builder.ConfigureOpenTelemetry(resource);
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder, ObservabilityResource resource)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(resource);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName: resource.ServiceName, serviceVersion: resource.ServiceVersion)
                .AddAttributes(
                [
                    new KeyValuePair<string, object>(LogFields.DeploymentEnvironment, resource.Environment),
                    new KeyValuePair<string, object>(LogFields.Squad, resource.Squad),
                ]))
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(resource.ServiceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = context => !HealthPaths.IsHealth(context.Request.Path);
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            var flightId = FlightId.FromHeaders(request.Headers);
                            if (flightId is not null)
                            {
                                FlightId.Set(activity, flightId);
                            }
                        };
                    })
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();
        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Maps <c>/health</c> (readiness) and <c>/alive</c> (liveness) in every
    /// environment so Container Apps probes work. Both stay anonymous (ADR-0018)
    /// and are excluded from traces and request logs.
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapHealthChecks(HealthPaths.Health);
        app.MapHealthChecks(HealthPaths.Alive, new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
        });

        return app;
    }
}
