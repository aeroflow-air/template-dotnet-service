using Microsoft.Extensions.Logging;

namespace AeroFlow.ServiceTemplate;

/// <summary>
/// Example business events for the demo bookings API. Service event ids start
/// at <see cref="AeroFlow.ServiceDefaults.PlatformEventIds.BusinessEventsStart"/>.
/// See docs/service-defaults.md.
/// </summary>
internal static partial class BookingLog
{
    [LoggerMessage(
        EventId = 10_000,
        Level = LogLevel.Information,
        Message = "Booking probe requested")]
    public static partial void Ping(ILogger logger);

    [LoggerMessage(
        EventId = 10_001,
        Level = LogLevel.Information,
        Message = "Booking {BookingId} looked up on flight {FlightNumber}")]
    public static partial void BookingLookedUp(ILogger logger, string bookingId, string flightNumber);

    [LoggerMessage(
        EventId = 10_002,
        Level = LogLevel.Information,
        Message = "Booking {BookingId} was not found")]
    public static partial void BookingNotFound(ILogger logger, string bookingId);
}
