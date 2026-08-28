namespace SynergieGlobalTutoringScheduling.Domain;

public static class BookingDurations
{
    public const int Min = 60;
    public const int Max = 90;

    public static bool IsAllowed(int durationMinutes)
    {
        return durationMinutes == Min || durationMinutes == Max;
    }
}
