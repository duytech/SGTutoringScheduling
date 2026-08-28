using SynergieGlobalTutoringScheduling.Contracts;
using SynergieGlobalTutoringScheduling.Domain;

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

        if (!BookingDurations.IsAllowed(request.DurationMinutes))
        {
            response.Errors.Add(CreateError(
                code: ValidationErrorCodes.InvalidDuration,
                message: $"Duration must be either {BookingDurations.SixtyMinutes} or {BookingDurations.NinetyMinutes} minutes."));
        }

        if (request.LessonDate.DayOfWeek == DayOfWeek.Monday)
        {
            response.Errors.Add(CreateError(
                code: ValidationErrorCodes.CentreClosedMonday,
                message: "The centre does not accept new bookings on Monday."));
        }

        var tutorExists = tutors.Any(tutor => tutor.Id == request.TutorId);
        if (!tutorExists)
        {
            response.Errors.Add(CreateError(
                code: ValidationErrorCodes.TutorNotFound,
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
            Severity = ValidationIssueSeverities.Error
        };
    }
}
