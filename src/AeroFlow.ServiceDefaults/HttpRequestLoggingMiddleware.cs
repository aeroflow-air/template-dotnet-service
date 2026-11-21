using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace AeroFlow.ServiceDefaults;

internal sealed class HttpRequestLoggingMiddleware(RequestDelegate next, IObservabilityLog log)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (HealthPaths.IsHealth(context.Request.Path))
        {
            await next(context);
            return;
        }

        var flightId = FlightId.Resolve(Activity.Current, context.Request.Headers);
        if (flightId is not null)
        {
            FlightId.Set(Activity.Current, flightId);
        }

        var started = Stopwatch.GetTimestamp();
        var path = context.Request.Path.HasValue ? context.Request.Path.Value! : "/";
        var method = context.Request.Method;
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (traceId, resolvedFlightId) = ReadContext(context, flightId);
            log.HttpRequestFailed(method, path, Stopwatch.GetElapsedTime(started).TotalMilliseconds, ex, traceId, resolvedFlightId);
            throw;
        }

        var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        var (completedTraceId, completedFlightId) = ReadContext(context, flightId);
        var handled = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (handled is not null)
        {
            log.HttpRequestFailed(method, path, elapsed, handled, completedTraceId, completedFlightId);
            return;
        }

        log.HttpRequestCompleted(method, path, context.Response.StatusCode, elapsed, completedTraceId, completedFlightId);
    }

    private static (string? TraceId, string? FlightId) ReadContext(HttpContext context, string? flightId)
    {
        var activity = Activity.Current;
        var traceId = activity is not null && activity.TraceId != default
            ? activity.TraceId.ToString()
            : context.TraceIdentifier;
        return (traceId, FlightId.Resolve(activity, context.Request.Headers) ?? flightId);
    }
}
