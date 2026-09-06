using System.Globalization;
using TutoringScheduling.Domain;
using TutoringScheduling.Services;

namespace TutoringScheduling.Tests.Support;

internal sealed class FixedClock : IClock
{
    public FixedClock(string centreLocalTime)
    {
        var local = DateTime.ParseExact(
            centreLocalTime, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        Now = new DateTimeOffset(local, CentreCalendar.TimeZoneOffset);
    }

    public DateTimeOffset Now { get; }
}
