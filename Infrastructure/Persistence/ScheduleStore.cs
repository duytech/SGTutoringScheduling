using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of the application's persistence port. This is the
/// only place a <see cref="AppDbContext"/> is touched on the read/write path.
/// </summary>
public sealed class ScheduleStore : IScheduleStore
{
    private readonly AppDbContext _db;

    public ScheduleStore(AppDbContext db)
    {
        _db = db;
    }

    public Task<Booking?> FindBookingAsync(string lessonId, CancellationToken cancellationToken = default)
    {
        return _db.Bookings
            .Include(booking => booking.Tutor)
            .FirstOrDefaultAsync(booking => booking.Id == lessonId, cancellationToken);
    }

    public Task<bool> RoomExistsAsync(string roomId, CancellationToken cancellationToken = default)
    {
        return _db.Rooms.AnyAsync(room => room.Id == roomId, cancellationToken);
    }

    public async Task<IReadOnlyList<Room>> GetRoomsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Rooms
            .OrderBy(room => room.Id)
            .ToListAsync(cancellationToken);
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

    public async Task<IReadOnlyList<LessonEvent>> GetLessonEventsAsync(
        string lessonId, CancellationToken cancellationToken = default)
    {
        return await _db.LessonEvents
            .Where(lessonEvent => lessonEvent.LessonId == lessonId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LessonEvent>> GetCutoffMovesTouchingDayAsync(
        DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _db.LessonEvents
            .Where(lessonEvent => lessonEvent.Type == LessonEventType.Moved && lessonEvent.AfterCutoff)
            .Where(lessonEvent => lessonEvent.ToDate == date || lessonEvent.FromDate == date)
            .ToListAsync(cancellationToken);
    }

    public async Task RecordMoveAsync(
        Booking movedLesson, LessonEvent moveEvent, CancellationToken cancellationToken = default)
    {
        _db.Bookings.Update(movedLesson);
        _db.LessonEvents.Add(moveEvent);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
