namespace TutoringScheduling.Application.Contracts;

public sealed class LessonHistoryResponse
{
    public required LessonDto Lesson { get; set; }

    public List<LessonEventDto> Events { get; set; } = [];
}
