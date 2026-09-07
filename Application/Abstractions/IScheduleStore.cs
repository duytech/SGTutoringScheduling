using TutoringScheduling.Domain;

namespace TutoringScheduling.Application.Abstractions;

/// <summary>
/// The persistence port the use-case layer depends on. Infrastructure supplies
/// the EF Core implementation; the application never sees a <c>DbContext</c>.
/// Query methods return fully materialised domain objects — no <c>IQueryable</c>
/// leaks across the boundary.
/// </summary>
public interface IScheduleStore
{
    /// <summary>The lesson and its tutor, or null if no lesson has that id.</summary>
    Task<Booking?> FindBookingAsync(string lessonId, CancellationToken cancellationToken = default);

    Task<bool> RoomExistsAsync(string roomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Room>> GetRoomsAsync(CancellationToken cancellationToken = default);

    /// <summary>Every booking on <paramref name="date"/>, tutor included.</summary>
    Task<IReadOnlyList<Booking>> GetBookingsForDayAsync(
        DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>Bookings whose date falls within the (inclusive, open-ended) range.</summary>
    Task<IReadOnlyList<Booking>> GetBookingsInRangeAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);

    /// <summary>Every event recorded against a lesson, in no particular order.</summary>
    Task<IReadOnlyList<LessonEvent>> GetLessonEventsAsync(
        string lessonId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <see cref="LessonEventType.Moved"/> events made past the day-before cut-off
    /// that either left <paramref name="date"/> or landed on it.
    /// </summary>
    Task<IReadOnlyList<LessonEvent>> GetCutoffMovesTouchingDayAsync(
        DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the rescheduled lesson and appends its move event as one unit of work.
    /// </summary>
    Task RecordMoveAsync(
        Booking movedLesson, LessonEvent moveEvent, CancellationToken cancellationToken = default);
}
