using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IBookingStore"/>. One of the few places
/// a <see cref="AppDbContext"/> is touched on the read path.
/// </summary>
public sealed class BookingStore : IBookingStore
{
    private readonly AppDbContext _db;

    public BookingStore(AppDbContext db)
    {
        _db = db;
    }

    public Task<Booking?> FindBookingAsync(string lessonId, CancellationToken cancellationToken = default)
    {
        return _db.Bookings
            .Include(booking => booking.Tutor)
            .FirstOrDefaultAsync(booking => booking.Id == lessonId, cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetBookingsForDayAsync(
        DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _db.Bookings
            .Include(booking => booking.Tutor)
            .Where(booking => booking.LessonDate == date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetBookingsInRangeAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var query = _db.Bookings.Include(booking => booking.Tutor).AsQueryable();

        if (from is { } fromDate)
        {
            query = query.Where(booking => booking.LessonDate >= fromDate);
        }

        if (to is { } toDate)
        {
            query = query.Where(booking => booking.LessonDate <= toDate);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
