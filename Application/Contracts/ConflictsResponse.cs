namespace TutoringScheduling.Application.Contracts;

public sealed class ConflictsResponse
{
    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public ConflictSummary Summary { get; set; } = new();

    public List<ConflictDto> Conflicts { get; set; } = [];
}

public sealed class ConflictSummary
{
    public int Errors { get; set; }

    public int Warnings { get; set; }
}
