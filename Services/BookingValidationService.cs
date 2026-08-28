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
        var bookings = await _scheduleStore.LoadBookingsAsync(cancellationToken);

        if (!BookingDurations.IsAllowed(request.DurationMinutes))
        {
            response.Errors.Add(CreateError(
                code: ValidationErrorCodes.InvalidDuration,
                message: $"Duration must be either {BookingDurations.Min} or {BookingDurations.Max} minutes."));
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

        var overlappingBookings = GetOverlappingBookings(bookings, request);

        var studentOverlapIds = overlappingBookings
            .Where(booking => booking.StudentName == request.StudentName)
            .Select(booking => booking.Id)
            .ToList();

        if (studentOverlapIds.Count > 0)
        {
            response.Errors.Add(CreateError(
                code: ValidationErrorCodes.StudentOverlap,
                message: $"Student '{request.StudentName}' already has another booking overlapping this time.",
                relatedBookingIds: studentOverlapIds));
        }

        var tutorOverlapIds = overlappingBookings
            .Where(booking => booking.TutorId == request.TutorId)
            .Select(booking => booking.Id)
            .ToList();

        if (tutorOverlapIds.Count > 0)
        {
            response.Errors.Add(CreateError(
                code: ValidationErrorCodes.TutorOverlap,
                message: $"Tutor '{request.TutorId}' already has another booking overlapping this time.",
                relatedBookingIds: tutorOverlapIds));
        }

        var roomOverlapIds = overlappingBookings
            .Where(booking => booking.Room == request.Room)
            .Select(booking => booking.Id)
            .ToList();

        if (roomOverlapIds.Count > 0)
        {
            response.Errors.Add(CreateError(
                code: ValidationErrorCodes.RoomOverlap,
                message: $"Room '{request.Room}' is already occupied during this time.",
                relatedBookingIds: roomOverlapIds));
        }

        response.Valid = response.Errors.Count == 0;
        return response;
    }

    private static List<Booking> GetOverlappingBookings(
        IReadOnlyList<Booking> bookings,
        ValidateBookingRequest request)
    {
        var requestStart = request.LessonDate.ToDateTime(request.StartTime);
        var requestEnd = requestStart.AddMinutes(request.DurationMinutes);

        return bookings
            .Where(booking => booking.Status != BookingStatus.Cancelled)
            .Where(booking => booking.LessonDate == request.LessonDate)
            .Where(booking => booking.GetStartDateTime() < requestEnd && requestStart < booking.GetEndDateTime())
            .ToList();
    }

    private static ValidationIssueDto CreateError(
        string code,
        string message,
        List<string>? relatedBookingIds = null)
    {
        return new ValidationIssueDto
        {
            Code = code,
            Message = message,
            Severity = ValidationIssueSeverities.Error,
            RelatedBookingIds = relatedBookingIds ?? []
        };
    }
}
