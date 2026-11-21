using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AeroFlow.ServiceDefaults;

internal sealed partial class ObservabilityLog : IObservabilityLog
{
    private readonly ILogger _logger;
    private readonly ObservabilityResource _resource;

    public ObservabilityLog(ILoggerFactory loggerFactory, ObservabilityResource resource)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(resource);
        _logger = loggerFactory.CreateLogger(LogFields.PlatformCategory);
        _resource = resource;
    }

    public void HttpRequestCompleted(string method, string path, int statusCode, double durationMs, string? traceId, string? flightId)
    {
        using var scope = BeginScope(traceId, flightId);
        var rounded = Math.Round(durationMs, 1);
        if (statusCode >= 500)
        {
            LogHttpRequestFailedStatus(method, path, statusCode, rounded);
            return;
        }

        LogHttpRequestCompleted(method, path, statusCode, rounded);
    }

    public void HttpRequestFailed(string method, string path, double durationMs, Exception exception, string? traceId, string? flightId)
    {
        using var scope = BeginScope(traceId, flightId);
        LogHttpRequestFailed(exception, method, path, Math.Round(durationMs, 1));
    }

    public void MessagePublished(string topic, string? subject, string? messageId, string? flightId)
    {
        using var scope = BeginScope(CurrentTraceId(), flightId);
        LogMessagePublished(topic, subject, messageId);
    }

    public void MessagePublishFailed(string topic, string? subject, string? messageId, Exception exception, string? flightId)
    {
        using var scope = BeginScope(CurrentTraceId(), flightId);
        LogMessagePublishFailed(exception, topic, subject, messageId);
    }

    public void MessageConsumed(string topic, string? subscription, string? subject, string? messageId, string outcome, string? flightId)
    {
        using var scope = BeginScope(CurrentTraceId(), flightId);
        LogMessageConsumed(topic, subscription, subject, messageId, outcome);
    }

    public void MessageConsumeFailed(string topic, string? subscription, string? subject, string? messageId, Exception exception, string? flightId)
    {
        using var scope = BeginScope(CurrentTraceId(), flightId);
        LogMessageConsumeFailed(exception, topic, subscription, subject, messageId);
    }

    public void OperationFailed(string operation, Exception exception, string? flightId)
    {
        using var scope = BeginScope(CurrentTraceId(), flightId);
        LogOperationFailed(exception, operation);
    }

    private IDisposable? BeginScope(string? traceId, string? flightId) =>
        _logger.BeginScope(_resource.ToScope(traceId, flightId));

    private static string? CurrentTraceId()
    {
        var activity = Activity.Current;
        return activity is null || activity.TraceId == default ? null : activity.TraceId.ToString();
    }

    [LoggerMessage(
        EventId = PlatformEventIds.HttpRequestCompleted,
        Level = LogLevel.Information,
        Message = "HTTP {Method} {Path} responded {StatusCode} in {DurationMs}ms")]
    private partial void LogHttpRequestCompleted(string method, string path, int statusCode, double durationMs);

    [LoggerMessage(
        EventId = PlatformEventIds.HttpRequestFailed,
        Level = LogLevel.Warning,
        Message = "HTTP {Method} {Path} responded {StatusCode} in {DurationMs}ms")]
    private partial void LogHttpRequestFailedStatus(string method, string path, int statusCode, double durationMs);

    [LoggerMessage(
        EventId = PlatformEventIds.HttpRequestUnhandled,
        Level = LogLevel.Error,
        Message = "HTTP {Method} {Path} failed in {DurationMs}ms")]
    private partial void LogHttpRequestFailed(Exception exception, string method, string path, double durationMs);

    [LoggerMessage(
        EventId = PlatformEventIds.MessagePublished,
        Level = LogLevel.Information,
        Message = "Published {Subject} to {Topic} ({MessageId})")]
    private partial void LogMessagePublished(string topic, string? subject, string? messageId);

    [LoggerMessage(
        EventId = PlatformEventIds.MessagePublishFailed,
        Level = LogLevel.Error,
        Message = "Failed to publish {Subject} to {Topic} ({MessageId})")]
    private partial void LogMessagePublishFailed(Exception exception, string topic, string? subject, string? messageId);

    [LoggerMessage(
        EventId = PlatformEventIds.MessageConsumed,
        Level = LogLevel.Information,
        Message = "Consumed {Subject} from {Topic}/{Subscription} ({MessageId}): {Outcome}")]
    private partial void LogMessageConsumed(string topic, string? subscription, string? subject, string? messageId, string outcome);

    [LoggerMessage(
        EventId = PlatformEventIds.MessageConsumeFailed,
        Level = LogLevel.Error,
        Message = "Failed to consume {Subject} from {Topic}/{Subscription} ({MessageId})")]
    private partial void LogMessageConsumeFailed(Exception exception, string topic, string? subscription, string? subject, string? messageId);

    [LoggerMessage(
        EventId = PlatformEventIds.OperationFailed,
        Level = LogLevel.Error,
        Message = "Operation {Operation} failed")]
    private partial void LogOperationFailed(Exception exception, string operation);
}
