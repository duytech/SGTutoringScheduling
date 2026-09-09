using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Application.Contracts;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Application;

/// <summary>
/// Read side of tutor workload. Split from the schedule board because it is an
/// independent view; the board still surfaces the same overload through the
/// <c>TUTOR_DAILY_LIMIT</c> conflict.
/// </summary>
public sealed class TutorService : ITutorService
{
    private readonly IBookingStore _bookings;

    public TutorService(IBookingStore bookings)
    {
        _bookings = bookings;
    }

    public async Task<TutorLoadsResponse> GetLoadsForDayAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var bookings = await _bookings.GetBookingsForDayAsync(date, cancellationToken);

        var tutorLoads = bookings
            .Where(booking => booking.Status != BookingStatus.Cancelled)
            .GroupBy(booking => (booking.TutorId, TutorName: booking.Tutor.Name))
            .OrderBy(group => group.Key.TutorId, StringComparer.Ordinal)
            .Select(group => new TutorLoadDto
            {
                TutorId = group.Key.TutorId,
                TutorName = group.Key.TutorName,
                LessonCount = group.Count(),
                Limit = BookingLimits.MaxBookingsPerTutorPerDay,
                OverLimit = group.Count() > BookingLimits.MaxBookingsPerTutorPerDay,
            })
            .ToList();

        return new TutorLoadsResponse
        {
            Date = date,
            TutorLoads = tutorLoads,
        };
    }
}
