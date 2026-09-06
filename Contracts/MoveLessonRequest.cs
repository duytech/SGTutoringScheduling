namespace TutoringScheduling.Contracts;

public sealed class MoveLessonRequest
{
    public DateOnly ToDate { get; set; }

    public TimeOnly ToStartTime { get; set; }

    /// <summary>Optional: keep the current room when omitted.</summary>
    public string? ToRoomId { get; set; }

    public string? Reason { get; set; }
}

public sealed class MoveLessonResponse
{
    public required LessonDto Lesson { get; set; }

    public required LessonEventDto Event { get; set; }

    /// <summary>Non-blocking conflicts the move still leaves in place (e.g. tutor over daily limit).</summary>
    public List<ConflictDto> Warnings { get; set; } = [];
}

public sealed class MoveRejectedResponse
{
    public required string Reason { get; set; }

    public List<ConflictDto> Conflicts { get; set; } = [];
}
