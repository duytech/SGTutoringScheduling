namespace TutoringScheduling.Contracts;

public sealed class LessonDto
{
    public required string Id { get; set; }

    public required DateOnly Date { get; set; }

    public required TimeOnly StartTime { get; set; }

    public required TimeOnly EndTime { get; set; }

    public int DurationMinutes { get; set; }

    public required string StudentName { get; set; }

    public required string TutorId { get; set; }

    public required string TutorName { get; set; }

    public required string RoomId { get; set; }

    public required string Status { get; set; }

    public string? GroupId { get; set; }

    public List<string> ConflictCodes { get; set; } = [];
}
