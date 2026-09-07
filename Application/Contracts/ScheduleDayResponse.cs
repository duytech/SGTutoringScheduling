namespace TutoringScheduling.Application.Contracts;

public sealed class ScheduleDayResponse
{
    public required DateOnly Date { get; set; }

    public bool IsMonday { get; set; }

    public List<RoomScheduleDto> Rooms { get; set; } = [];

    public List<TutorLoadDto> TutorLoads { get; set; } = [];

    public List<ConflictDto> Conflicts { get; set; } = [];

    /// <summary>
    /// Moves touching this day that were made after 16:00 the day before — the
    /// changes a tutor needs to be told about rather than discover.
    /// </summary>
    public List<LessonEventDto> Changes { get; set; } = [];
}

public sealed class RoomScheduleDto
{
    public required string RoomId { get; set; }

    public required string RoomName { get; set; }

    public List<LessonDto> Lessons { get; set; } = [];
}

public sealed class TutorLoadDto
{
    public required string TutorId { get; set; }

    public required string TutorName { get; set; }

    public int LessonCount { get; set; }

    public int Limit { get; set; }

    public bool OverLimit { get; set; }
}
