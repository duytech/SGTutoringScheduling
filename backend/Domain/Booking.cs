namespace TutoringScheduling.Domain;

public sealed class Booking
{
    public string Id { get; set; } = null!;

    public string StudentName { get; set; } = null!;

    public DateOnly LessonDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public int DurationMinutes { get; set; }

    public string TutorId { get; set; } = null!;

    public string RoomId { get; set; } = null!;

    public BookingStatus Status { get; set; }

    /// <summary>
    /// Links lessons that share a tutor, room and slot on purpose (exam pairs).
    /// Null for an ordinary lesson. Set only by an explicit pairing action,
    /// never inferred from a coincidental duplicate.
    /// </summary>
    public string? GroupId { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public string? Note { get; set; }

    public Tutor Tutor { get; set; } = null!;

    public Room Room { get; set; } = null!;

    public DateTime GetStartDateTime()
    {
        return LessonDate.ToDateTime(StartTime);
    }

    public DateTime GetEndDateTime()
    {
        return GetStartDateTime().AddMinutes(DurationMinutes);
    }
}
