namespace TutoringScheduling.Domain;

/// <summary>
/// Fixed facts about how the centre runs its week.
/// </summary>
public static class CentreCalendar
{
    /// <summary>Da Nang is UTC+7 and does not observe daylight saving.</summary>
    public static readonly TimeSpan TimeZoneOffset = TimeSpan.FromHours(7);

    /// <summary>Changes are "visible as changes" once this time the day before passes.</summary>
    public static readonly TimeOnly ChangeCutoffTime = new(16, 0);

    /// <summary>
    /// The instant after which a change to a lesson on <paramref name="lessonDate"/>
    /// counts as something the tutor was already told about.
    /// </summary>
    public static DateTimeOffset ChangeCutoff(DateOnly lessonDate)
    {
        return new DateTimeOffset(lessonDate.AddDays(-1), ChangeCutoffTime, TimeZoneOffset);
    }

    public static bool IsClosed(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Monday;
    }
}
