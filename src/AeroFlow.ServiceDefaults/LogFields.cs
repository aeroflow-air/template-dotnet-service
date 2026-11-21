namespace AeroFlow.ServiceDefaults;

/// <summary>
/// Canonical structured-log field names. Platform logs and the contract-test
/// helper use these keys so every service is searchable the same way.
/// </summary>
public static class LogFields
{
    public const string PlatformCategory = "AeroFlow.ServiceDefaults";

    public const string ServiceName = "service.name";
    public const string ServiceVersion = "service.version";
    public const string DeploymentEnvironment = "deployment.environment";
    public const string Squad = "aeroflow.squad";
    public const string TraceId = "trace.id";
    public const string FlightId = "flight.id";

    public const string HttpMethod = "http.method";
    public const string HttpPath = "http.path";
    public const string HttpStatusCode = "http.status_code";
    public const string DurationMs = "duration_ms";

    public const string MessagingDestination = "messaging.destination.name";
    public const string MessagingSubscription = "messaging.subscription.name";
    public const string MessagingMessageId = "messaging.message.id";
    public const string MessagingOperation = "messaging.operation";
    public const string MessagingOutcome = "messaging.outcome";

    public const string Operation = "operation";
}

/// <summary>Outcome values for <see cref="LogFields.MessagingOutcome"/>.</summary>
public static class MessageOutcomes
{
    public const string Applied = "applied";
    public const string Rejected = "rejected";
    public const string Ignored = "ignored";
    public const string DeadLettered = "dead-lettered";
}

/// <summary>Reserved event ids for platform logs. Business events start at <see cref="BusinessEventsStart"/>.</summary>
public static class PlatformEventIds
{
    public const int HttpRequestCompleted = 1000;
    public const int HttpRequestFailed = 1001;
    public const int HttpRequestUnhandled = 1002;
    public const int MessagePublished = 2000;
    public const int MessagePublishFailed = 2001;
    public const int MessageConsumed = 2002;
    public const int MessageConsumeFailed = 2003;
    public const int OperationFailed = 3000;

    /// <summary>First event id available for a service's own <c>[LoggerMessage]</c> methods.</summary>
    public const int BusinessEventsStart = 10_000;
}
