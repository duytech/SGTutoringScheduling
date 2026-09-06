namespace SynergieGlobalTutoringScheduling.Contracts;

/// <summary>
/// One detected clash in the existing schedule. Errors are physical
/// impossibilities; warnings are policy breaches the centre sometimes makes
/// on purpose.
/// </summary>
public sealed class ConflictDto
{
    public required string Code { get; set; }

    public required string Severity { get; set; }

    public required DateOnly Date { get; set; }

    public required string Message { get; set; }

    public List<string> BookingIds { get; set; } = [];

    public string? TutorId { get; set; }

    public string? RoomId { get; set; }

    public string? StudentName { get; set; }
}
