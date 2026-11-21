namespace AeroFlow.ServiceDefaults;

/// <summary>
/// Platform logs emitted by this package. Services use this for messaging and
/// unexpected failures; HTTP request logs are written automatically.
/// Business events stay in the service as <c>[LoggerMessage]</c> methods with
/// event ids at or above <see cref="PlatformEventIds.BusinessEventsStart"/>.
/// </summary>
public interface IObservabilityLog
{
    void HttpRequestCompleted(string method, string path, int statusCode, double durationMs, string? traceId, string? flightId);

    void HttpRequestFailed(string method, string path, double durationMs, Exception exception, string? traceId, string? flightId);

    void MessagePublished(string topic, string? subject, string? messageId, string? flightId);

    void MessagePublishFailed(string topic, string? subject, string? messageId, Exception exception, string? flightId);

    void MessageConsumed(string topic, string? subscription, string? subject, string? messageId, string outcome, string? flightId);

    void MessageConsumeFailed(string topic, string? subscription, string? subject, string? messageId, Exception exception, string? flightId);

    void OperationFailed(string operation, Exception exception, string? flightId);
}
