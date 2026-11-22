namespace AeroFlow.ServiceTemplate.Domain;

/// <summary>
/// Placeholder pure domain type, like <c>BookingsController</c> and <c>Models/</c>: replace it with your own domain.
/// The caller reads the clock at the edge (inject <see cref="TimeProvider"/> there) and passes <c>now</c> in;
/// the domain never reads it. <c>DomainPurityTests</c> enforce this for everything in <c>Domain/</c>.
/// </summary>
public sealed record CheckInWindow(DateTimeOffset Opens, DateTimeOffset Closes)
{
    /// <summary>Half-open: open from <see cref="Opens"/> up to, not including, <see cref="Closes"/>.</summary>
    public bool IsOpenAt(DateTimeOffset now) => now >= Opens && now < Closes;
}
