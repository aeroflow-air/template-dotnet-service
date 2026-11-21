namespace AeroFlow.ServiceDefaults;

/// <summary>
/// Optional overrides for the required resource attributes (ADR-0022).
/// Bound from the <c>Observability</c> configuration section.
/// </summary>
public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    /// <summary>Maps to <c>service.name</c>. Defaults to the host application name.</summary>
    public string? ServiceName { get; set; }

    /// <summary>Maps to <c>service.version</c>. Defaults to the entry assembly informational version.</summary>
    public string? ServiceVersion { get; set; }

    /// <summary>Maps to <c>deployment.environment</c>. Defaults to the host environment name.</summary>
    public string? Environment { get; set; }

    /// <summary>Maps to <c>aeroflow.squad</c>. Defaults to <c>unknown</c> if unset.</summary>
    public string? Squad { get; set; }
}
