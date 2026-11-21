using AeroFlow.ServiceDefaults.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace AeroFlow.ServiceDefaults.Tests;

public sealed class HttpRequestLoggingTests
{
    [Fact]
    public async Task Request_log_includes_required_fields_and_flight_id()
    {
        await using var host = await TestApp.StartAsync(map: app =>
        {
            app.MapGet("/ping", () => "ok");
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, "/ping");
        request.Headers.Add(FlightId.HeaderName, "AF204");
        using var response = await host.Client.SendAsync(request);

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        host.Logs
            .RequireHttpRequest("GET", "/ping", StatusCodes.Status200OK)
            .RequireRequiredFields(host.Resource)
            .RequireFlightId("AF204");
    }

    [Fact]
    public async Task Five_hundred_is_logged_as_a_failed_request()
    {
        await using var host = await TestApp.StartAsync(map: app =>
        {
            app.MapGet("/boom-status", () => Results.StatusCode(StatusCodes.Status500InternalServerError));
        });

        using var response = await host.Client.GetAsync("/boom-status");
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);

        var log = host.Logs.RequireHttpRequest("GET", "/boom-status", StatusCodes.Status500InternalServerError);
        log.RequireRequiredFields(host.Resource);
        Assert.Equal(PlatformEventIds.HttpRequestFailed, log.EventId.Id);
        Assert.Equal(LogLevel.Warning, log.Level);
    }

    [Fact]
    public async Task Unhandled_exception_is_logged_as_event_1002()
    {
        // Production: no DeveloperExceptionPage, so the exception reaches our middleware
        // (or UseExceptionHandler, which exposes it as IExceptionHandlerFeature).
        await using var host = await TestApp.StartAsync(
            environment: "Production",
            map: app =>
            {
                app.MapGet("/boom", () =>
                {
                    throw new InvalidOperationException("probe failure");
                });
            });

        using var response = await host.Client.GetAsync("/boom");
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);

        var log = host.Logs.RequireEvent(PlatformEventIds.HttpRequestUnhandled);
        log.RequireRequiredFields(host.Resource);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.IsType<InvalidOperationException>(log.Exception);
        Assert.Equal("probe failure", log.Exception!.Message);
        Assert.Equal("GET", log.Fields["Method"]?.ToString());
        Assert.Equal("/boom", log.Fields["Path"]?.ToString());
    }

    [Fact]
    public async Task Health_endpoints_are_not_request_logged_and_work_in_production()
    {
        await using var host = await TestApp.StartAsync(
            environment: "Production",
            configuration: new Dictionary<string, string?>
            {
                ["Observability:Environment"] = "Production",
            },
            map: app => app.MapGet("/ping", () => "ok"));

        using var health = await host.Client.GetAsync("/health");
        using var alive = await host.Client.GetAsync("/alive");
        using var ping = await host.Client.GetAsync("/ping");

        Assert.Equal(StatusCodes.Status200OK, (int)health.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, (int)alive.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, (int)ping.StatusCode);

        host.Logs.RequireNoHealthRequestLogs();
        host.Logs.RequireHttpRequest("GET", "/ping", StatusCodes.Status200OK);
    }
}
