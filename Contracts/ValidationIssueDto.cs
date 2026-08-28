namespace SynergieGlobalTutoringScheduling.Contracts;

public sealed class ValidationIssueDto
{
    public required string Code { get; set; }

    public required string Message { get; set; }

    public required string Severity { get; set; }

    public List<string> RelatedBookingIds { get; set; } = [];
}
