namespace TutoringScheduling.Contracts;

public sealed class ValidateBookingResponse
{
    public bool Valid { get; set; }

    public List<ValidationIssueDto> Errors { get; set; } = [];

    public List<ValidationIssueDto> Warnings { get; set; } = [];
}
