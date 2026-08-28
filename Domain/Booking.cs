namespace SynergieGlobalTutoringScheduling.Domain;

public sealed class Booking
{
    public string Id { get; set; }

    public string StudentName { get; set; }

    public DateOnly LessonDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public int DurationMinutes { get; set; }

    public string TutorId { get; set; }

    public string Room { get; set; }

    public BookingStatus Status { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public string? Note { get; set; }
}
