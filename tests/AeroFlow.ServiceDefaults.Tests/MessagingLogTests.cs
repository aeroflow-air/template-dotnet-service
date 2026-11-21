using System.Diagnostics;
using AeroFlow.ServiceDefaults.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AeroFlow.ServiceDefaults.Tests;

public sealed class MessagingLogTests
{
    [Fact]
    public async Task Publish_and_consume_logs_include_required_fields()
    {
        await using var host = await TestApp.StartAsync();
        using var activity = new Activity("messaging-test").Start();

        host.ObservabilityLog.MessagePublished("gate-events", "GateAssigned", "evt-1", "AF204");
        host.ObservabilityLog.MessageConsumed(
            "flight-events",
            "gate-allocation",
            "Landed",
            "evt-2",
            MessageOutcomes.Applied,
            "AF204");

        host.Logs
            .RequireEvent(PlatformEventIds.MessagePublished)
            .RequireRequiredFields(host.Resource)
            .RequireFlightId("AF204");
        Assert.Equal("gate-events", host.Logs.RequireEvent(PlatformEventIds.MessagePublished).Fields["Topic"]?.ToString());

        host.Logs
            .RequireEvent(PlatformEventIds.MessageConsumed)
            .RequireRequiredFields(host.Resource)
            .RequireFlightId("AF204");
        var consumed = host.Logs.RequireEvent(PlatformEventIds.MessageConsumed);
        Assert.Equal("flight-events", consumed.Fields["Topic"]?.ToString());
        Assert.Equal("gate-allocation", consumed.Fields["Subscription"]?.ToString());
        Assert.Equal(MessageOutcomes.Applied, consumed.Fields["Outcome"]?.ToString());
    }

    [Fact]
    public async Task Publish_and_consume_failures_are_logged_with_the_exception()
    {
        await using var host = await TestApp.StartAsync();
        var error = new InvalidOperationException("bus down");

        host.ObservabilityLog.MessagePublishFailed("baggage-events", "BeltAssigned", "evt-3", error, "AF9");
        host.ObservabilityLog.MessageConsumeFailed("flight-events", "baggage-reclaim", "Landed", "evt-4", error, "AF9");
        host.ObservabilityLog.OperationFailed("outbox-dispatch", error, "AF9");

        foreach (var eventId in new[]
                 {
                     PlatformEventIds.MessagePublishFailed,
                     PlatformEventIds.MessageConsumeFailed,
                     PlatformEventIds.OperationFailed,
                 })
        {
            var log = host.Logs.RequireEvent(eventId);
            log.RequireRequiredFields(host.Resource, requireTraceId: false);
            log.RequireFlightId("AF9");
            Assert.Same(error, log.Exception);
            Assert.Equal(LogLevel.Error, log.Level);
        }
    }
}
