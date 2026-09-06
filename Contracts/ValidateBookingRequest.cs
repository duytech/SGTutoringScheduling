namespace TutoringScheduling.Contracts;

public sealed class ValidateBookingRequest
{
    public required string StudentName { get; set; }

    public required string TutorId { get; set; }

    public required string Room { get; set; }

    public required DateOnly LessonDate { get; set; }

    public required TimeOnly StartTime { get; set; }

    public int DurationMinutes { get; set; }
}
