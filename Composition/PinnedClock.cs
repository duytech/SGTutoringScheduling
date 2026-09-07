using System.Globalization;
using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Composition;

/// <summary>
/// "Now" is pinned to a value inside the seeded week (config key
/// <c>Schedule:Now</c>), never the real system clock — the brief requires it.
/// </summary>
public sealed class PinnedClock : IClock
{
    public const string DefaultNow = "2026-03-06T09:00:00";

    public PinnedClock(IConfiguration configuration)
    {
        var raw = configuration["Schedule:Now"] ?? DefaultNow;
        var local = DateTime.ParseExact(
            raw, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        Now = new DateTimeOffset(local, CentreCalendar.TimeZoneOffset);
    }

    public DateTimeOffset Now { get; }
}
