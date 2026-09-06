namespace TutoringScheduling.Domain;

public enum LessonEventType
{
    Created,
    Moved,
}

/// <summary>
/// An append-only record of something that happened to a lesson. The
/// <see cref="Booking"/> row holds the current state; the events hold the
/// history, so a move never silently overwrites what a tutor was told.
/// </summary>
public sealed class LessonEvent
{
    public int Id { get; set; }

    public string LessonId { get; set; } = null!;

    public LessonEventType Type { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    // Set for a Moved event; null otherwise.
    public DateOnly? FromDate { get; set; }

    public TimeOnly? FromStartTime { get; set; }

    public string? FromRoomId { get; set; }

    public DateOnly? ToDate { get; set; }

    public TimeOnly? ToStartTime { get; set; }

    public string? ToRoomId { get; set; }

    public string? Reason { get; set; }

    /// <summary>
    /// True when the change was made after 16:00 the day before the lesson —
    /// past the point where the tutor has already been told the schedule, so
    /// it must be shown as a change rather than applied silently.
    /// </summary>
    public bool AfterCutoff { get; set; }

    public Booking Lesson { get; set; } = null!;
}
