using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace AeroFlow.ServiceDefaults;

/// <summary>
/// Flight id is an operational identifier, not PII. It belongs on traces and
/// logs, never as a metric dimension (ADR-0022).
/// </summary>
public static class FlightId
{
    public const string HeaderName = "X-Flight-Id";
    public const string ActivityTag = "aeroflow.flight_id";
    public const string BaggageName = "aeroflow.flight_id";

    public static string? FromActivity(Activity? activity)
    {
        if (activity is null)
        {
            return null;
        }

        var tagged = activity.GetTagItem(ActivityTag) as string;
        if (!string.IsNullOrWhiteSpace(tagged))
        {
            return tagged;
        }

        var baggage = activity.GetBaggageItem(BaggageName);
        return string.IsNullOrWhiteSpace(baggage) ? null : baggage;
    }

    public static string? FromHeaders(IHeaderDictionary? headers)
    {
        if (headers is null || !headers.TryGetValue(HeaderName, out var values))
        {
            return null;
        }

        var value = values.ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static string? Resolve(Activity? activity, IHeaderDictionary? headers) =>
        FromActivity(activity) ?? FromHeaders(headers);

    public static void Set(Activity? activity, string flightId)
    {
        if (activity is null || string.IsNullOrWhiteSpace(flightId))
        {
            return;
        }

        activity.SetTag(ActivityTag, flightId);
        if (string.IsNullOrEmpty(activity.GetBaggageItem(BaggageName)))
        {
            activity.SetBaggage(BaggageName, flightId);
        }
    }
}
