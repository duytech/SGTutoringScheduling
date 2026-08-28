namespace SynergieGlobalTutoringScheduling.Domain;

public static class BookingDurations
{
    public const int SixtyMinutes = 60;
    public const int NinetyMinutes = 90;

    public static bool IsAllowed(int durationMinutes)
    {
        return durationMinutes == SixtyMinutes || durationMinutes == NinetyMinutes;
    }
}
