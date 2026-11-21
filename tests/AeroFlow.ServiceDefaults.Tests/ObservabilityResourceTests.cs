using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AeroFlow.ServiceDefaults.Tests;

public sealed class ObservabilityResourceTests
{
    [Fact]
    public void Resolve_prefers_options_then_falls_back_cleanly()
    {
        var resource = ObservabilityResource.Resolve(
            new StubHostEnvironment("GateAllocation", "Development"),
            new ConfigurationBuilder().Build(),
            new ObservabilityOptions
            {
                ServiceName = "svc-gate-allocation",
                ServiceVersion = "0.4.0",
                Environment = "local",
                Squad = "platform",
            },
            entryAssembly: null);

        Assert.Equal("svc-gate-allocation", resource.ServiceName);
        Assert.Equal("0.4.0", resource.ServiceVersion);
        Assert.Equal("local", resource.Environment);
        Assert.Equal("platform", resource.Squad);
    }

    [Fact]
    public void Resolve_defaults_squad_to_unknown_and_uses_host_name()
    {
        var resource = ObservabilityResource.Resolve(
            new StubHostEnvironment("AeroFlow.ServiceTemplate", "Production"),
            new ConfigurationBuilder().Build(),
            new ObservabilityOptions(),
            entryAssembly: null);

        Assert.Equal("AeroFlow.ServiceTemplate", resource.ServiceName);
        Assert.Equal("Production", resource.Environment);
        Assert.Equal("unknown", resource.Squad);
        Assert.Equal("0.0.0", resource.ServiceVersion);
    }

    [Fact]
    public void ToScope_omits_absent_flight_and_trace_ids()
    {
        var resource = new ObservabilityResource("svc-x", "1.0.0", "local", "platform");
        var scope = resource.ToScope(traceId: null, flightId: null);

        Assert.Equal("svc-x", scope[LogFields.ServiceName]);
        Assert.False(scope.ContainsKey(LogFields.TraceId));
        Assert.False(scope.ContainsKey(LogFields.FlightId));
    }

    [Fact]
    public void Flight_id_prefers_activity_tag_then_header()
    {
        using var activity = new Activity("flight").Start();
        FlightId.Set(activity, "AF1");
        Assert.Equal("AF1", FlightId.FromActivity(activity));
        Assert.Equal("AF1", FlightId.Resolve(activity, headers: null));
    }

    private sealed class StubHostEnvironment(string applicationName, string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = applicationName;

        public string ContentRootPath { get; set; } = "/";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
