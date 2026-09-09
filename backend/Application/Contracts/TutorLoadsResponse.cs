namespace TutoringScheduling.Application.Contracts;

public sealed class TutorLoadsResponse
{
    public required DateOnly Date { get; set; }

    public List<TutorLoadDto> TutorLoads { get; set; } = [];
}

public sealed class TutorLoadDto
{
    public required string TutorId { get; set; }

    public required string TutorName { get; set; }

    public int LessonCount { get; set; }

    public int Limit { get; set; }

    public bool OverLimit { get; set; }
}
