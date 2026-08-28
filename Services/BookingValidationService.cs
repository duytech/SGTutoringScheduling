using SynergieGlobalTutoringScheduling.Contracts;

namespace SynergieGlobalTutoringScheduling.Services;

public sealed class BookingValidationService
{
    private readonly JsonScheduleStore _scheduleStore;

    public BookingValidationService(JsonScheduleStore scheduleStore)
    {
        _scheduleStore = scheduleStore;
    }

    public async Task<ValidateBookingResponse> ValidateAsync(
        ValidateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new ValidateBookingResponse();
        var tutors = await _scheduleStore.LoadTutorsAsync(cancellationToken);

        if (request.DurationMinutes is not 60 and not 90)
        {
            response.Errors.Add(CreateError(
                code: "INVALID_DURATION",
                message: "Duration must be either 60 or 90 minutes."));
        }

        if (request.LessonDate.DayOfWeek == DayOfWeek.Monday)
        {
            response.Errors.Add(CreateError(
                code: "CENTRE_CLOSED_MONDAY",
                message: "The centre does not accept new bookings on Monday."));
        }

        var tutorExists = tutors.Any(tutor => tutor.Id == request.TutorId);
        if (!tutorExists)
        {
            response.Errors.Add(CreateError(
                code: "TUTOR_NOT_FOUND",
                message: $"Tutor '{request.TutorId}' does not exist."));
        }

        response.Valid = response.Errors.Count == 0;
        return response;
    }

    private static ValidationIssueDto CreateError(string code, string message)
    {
        return new ValidationIssueDto
        {
            Code = code,
            Message = message,
            Severity = "error"
        };
    }
}
