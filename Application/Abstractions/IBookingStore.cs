using TutoringScheduling.Domain;

namespace TutoringScheduling.Application.Abstractions;

/// <summary>
/// Read access to bookings. Infrastructure supplies the EF Core implementation;
/// the application never sees a <c>DbContext</c>. Methods return fully
/// materialised domain objects — no <c>IQueryable</c> leaks across the boundary.
/// </summary>
public interface IBookingStore
{
    /// <summary>The lesson and its tutor, or null if no lesson has that id.</summary>
    Task<Booking?> FindBookingAsync(string lessonId, CancellationToken cancellationToken = default);

    /// <summary>Every booking on <paramref name="date"/>, tutor included.</summary>
    Task<IReadOnlyList<Booking>> GetBookingsForDayAsync(
        DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>Bookings whose date falls within the (inclusive, open-ended) range.</summary>
    Task<IReadOnlyList<Booking>> GetBookingsInRangeAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}
