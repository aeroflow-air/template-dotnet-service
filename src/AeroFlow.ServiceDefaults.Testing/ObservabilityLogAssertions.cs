namespace AeroFlow.ServiceDefaults.Testing;

/// <summary>
/// Contract checks for platform logs. Throws <see cref="InvalidOperationException"/>
/// with a readable message so any test framework can use them.
/// </summary>
public static class ObservabilityLogAssertions
{
    public static IReadOnlyList<CollectedLog> PlatformLogs(this LogCollector collector) =>
        collector.Entries.Where(e => e.Category == LogFields.PlatformCategory).ToArray();

    public static CollectedLog RequireHttpRequest(
        this LogCollector collector,
        string method,
        string path,
        int statusCode)
    {
        var match = collector.PlatformLogs().FirstOrDefault(log =>
            log.EventId.Id is PlatformEventIds.HttpRequestCompleted or PlatformEventIds.HttpRequestFailed or PlatformEventIds.HttpRequestUnhandled
            && string.Equals(GetString(log, "Method"), method, StringComparison.OrdinalIgnoreCase)
            && string.Equals(GetString(log, "Path"), path, StringComparison.Ordinal)
            && GetInt(log, "StatusCode") == statusCode);

        if (match is null)
        {
            throw new InvalidOperationException(
                $"Expected a platform HTTP log for {method} {path} => {statusCode}. Captured:{Format(collector)}");
        }

        return match;
    }

    public static CollectedLog RequireEvent(this LogCollector collector, int eventId)
    {
        var match = collector.PlatformLogs().FirstOrDefault(log => log.EventId.Id == eventId);
        if (match is null)
        {
            throw new InvalidOperationException(
                $"Expected a platform log with event id {eventId}. Captured:{Format(collector)}");
        }

        return match;
    }

    public static CollectedLog RequireRequiredFields(
        this CollectedLog log,
        ObservabilityResource? expected = null,
        bool requireTraceId = true)
    {
        RequireField(log, LogFields.ServiceName, expected?.ServiceName);
        RequireField(log, LogFields.ServiceVersion, expected?.ServiceVersion);
        RequireField(log, LogFields.DeploymentEnvironment, expected?.Environment);
        RequireField(log, LogFields.Squad, expected?.Squad);

        if (requireTraceId && !log.Fields.ContainsKey(LogFields.TraceId))
        {
            throw new InvalidOperationException(
                $"Expected field '{LogFields.TraceId}' on event {log.EventId.Id}: {log.Message}");
        }

        return log;
    }

    public static CollectedLog RequireFlightId(this CollectedLog log, string flightId)
    {
        RequireField(log, LogFields.FlightId, flightId);
        return log;
    }

    public static void RequireNoHealthRequestLogs(this LogCollector collector)
    {
        var hits = collector.PlatformLogs()
            .Where(log => log.EventId.Id is PlatformEventIds.HttpRequestCompleted or PlatformEventIds.HttpRequestFailed or PlatformEventIds.HttpRequestUnhandled)
            .Where(log =>
            {
                var path = GetString(log, "Path") ?? "";
                return path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("/alive", StringComparison.OrdinalIgnoreCase);
            })
            .ToArray();

        if (hits.Length > 0)
        {
            throw new InvalidOperationException(
                $"Health endpoints must not be request-logged. Captured:{Format(collector)}");
        }
    }

    private static void RequireField(CollectedLog log, string name, string? expected)
    {
        if (!log.Fields.TryGetValue(name, out var value) || value is null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            throw new InvalidOperationException(
                $"Expected field '{name}' on event {log.EventId.Id}: {log.Message}. Fields: {string.Join(", ", log.Fields.Keys)}");
        }

        if (expected is not null && !string.Equals(value.ToString(), expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Expected field '{name}' to be '{expected}' but was '{value}' on event {log.EventId.Id}: {log.Message}");
        }
    }

    private static string? GetString(CollectedLog log, string key) =>
        log.Fields.TryGetValue(key, out var value) ? value?.ToString() : null;

    private static int? GetInt(CollectedLog log, string key) =>
        log.Fields.TryGetValue(key, out var value) && value is not null && int.TryParse(value.ToString(), out var parsed)
            ? parsed
            : null;

    private static string Format(LogCollector collector) =>
        collector.Entries.Count == 0
            ? " (none)"
            : Environment.NewLine + string.Join(
                Environment.NewLine,
                collector.Entries.Select(e => $"  [{e.Level}] {e.EventId.Id} {e.Category}: {e.Message}"));
}
