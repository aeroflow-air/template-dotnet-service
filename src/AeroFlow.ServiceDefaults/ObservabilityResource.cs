using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AeroFlow.ServiceDefaults;

/// <summary>Resolved, immutable resource attributes attached to every platform signal.</summary>
public sealed record ObservabilityResource(
    string ServiceName,
    string ServiceVersion,
    string Environment,
    string Squad)
{
    public static ObservabilityResource Resolve(
        IHostEnvironment hostEnvironment,
        IConfiguration configuration,
        ObservabilityOptions options,
        Assembly? entryAssembly)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(options);

        var serviceName = FirstNonEmpty(
            options.ServiceName,
            configuration["OTEL_SERVICE_NAME"],
            hostEnvironment.ApplicationName) ?? "unknown";

        var serviceVersion = FirstNonEmpty(
            options.ServiceVersion,
            ReadInformationalVersion(entryAssembly),
            entryAssembly?.GetName().Version?.ToString()) ?? "0.0.0";

        var environment = FirstNonEmpty(
            options.Environment,
            configuration["DOTNET_ENVIRONMENT"],
            hostEnvironment.EnvironmentName) ?? "Production";

        var squad = FirstNonEmpty(options.Squad) ?? "unknown";

        return new ObservabilityResource(serviceName, serviceVersion, environment, squad);
    }

    public IReadOnlyDictionary<string, object> ToScope(string? traceId, string? flightId)
    {
        var scope = new Dictionary<string, object>(6)
        {
            [LogFields.ServiceName] = ServiceName,
            [LogFields.ServiceVersion] = ServiceVersion,
            [LogFields.DeploymentEnvironment] = Environment,
            [LogFields.Squad] = Squad,
        };

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            scope[LogFields.TraceId] = traceId;
        }

        if (!string.IsNullOrWhiteSpace(flightId))
        {
            scope[LogFields.FlightId] = flightId;
        }

        return scope;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static string? ReadInformationalVersion(Assembly? assembly)
    {
        var version = assembly
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(version))
        {
            return null;
        }

        // Strip the +git-sha suffix the SDK sometimes appends so logs stay short and stable.
        var plus = version.IndexOf('+', StringComparison.Ordinal);
        return plus >= 0 ? version[..plus] : version;
    }
}
