namespace TutoringScheduling.Contracts;

public sealed class LessonEventDto
{
    public required string LessonId { get; set; }

    public required string Type { get; set; }

    public required DateTimeOffset OccurredAt { get; set; }

    public DateOnly? FromDate { get; set; }

    public TimeOnly? FromStartTime { get; set; }

    public string? FromRoomId { get; set; }

    public DateOnly? ToDate { get; set; }

    public TimeOnly? ToStartTime { get; set; }

    public string? ToRoomId { get; set; }

    public string? Reason { get; set; }

    public bool AfterCutoff { get; set; }
}
