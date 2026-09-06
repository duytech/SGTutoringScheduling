using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Contracts;
using TutoringScheduling.Data;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Services;

/// <summary>
/// Read side of the schedule: runs the conflict engine over stored bookings
/// and shapes the results for the API.
/// </summary>
public sealed class ScheduleService
{
    private readonly AppDbContext _db;

    public ScheduleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ScheduleDayResponse> GetDayAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var rooms = await _db.Rooms
            .OrderBy(room => room.Id)
            .ToListAsync(cancellationToken);

        var bookings = await _db.Bookings
            .Include(booking => booking.Tutor)
            .Where(booking => booking.LessonDate == date)
            .ToListAsync(cancellationToken);

        var conflicts = ConflictDetector.Detect(bookings);
        var codesByBookingId = MapConflictCodesByBooking(conflicts);

        var roomSchedules = rooms
            .Select(room => new RoomScheduleDto
            {
                RoomId = room.Id,
                RoomName = room.Name,
                Lessons = bookings
                    .Where(booking => booking.RoomId == room.Id)
                    .OrderBy(booking => booking.StartTime)
                    .ThenBy(booking => booking.Id, StringComparer.Ordinal)
                    .Select(booking => LessonMapper.ToDto(booking, codesByBookingId.GetValueOrDefault(booking.Id)))
                    .ToList(),
            })
            .ToList();

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

        return new ScheduleDayResponse
        {
            Date = date,
            IsMonday = date.DayOfWeek == DayOfWeek.Monday,
            Rooms = roomSchedules,
            TutorLoads = tutorLoads,
            Conflicts = conflicts.ToList(),
        };
    }

    public async Task<ConflictsResponse> GetConflictsAsync(
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Bookings.AsQueryable();

        if (from is { } fromDate)
        {
            query = query.Where(booking => booking.LessonDate >= fromDate);
        }

        if (to is { } toDate)
        {
            query = query.Where(booking => booking.LessonDate <= toDate);
        }

        var bookings = await query.ToListAsync(cancellationToken);
        var conflicts = ConflictDetector.Detect(bookings);

        return new ConflictsResponse
        {
            From = from,
            To = to,
            Summary = new ConflictSummary
            {
                Errors = conflicts.Count(c => c.Severity == ConflictSeverities.Error),
                Warnings = conflicts.Count(c => c.Severity == ConflictSeverities.Warning),
            },
            Conflicts = conflicts.ToList(),
        };
    }

    private static Dictionary<string, List<string>> MapConflictCodesByBooking(
        IReadOnlyList<ConflictDto> conflicts)
    {
        var codesByBookingId = new Dictionary<string, List<string>>();

        foreach (var conflict in conflicts)
        {
            foreach (var bookingId in conflict.BookingIds)
            {
                if (!codesByBookingId.TryGetValue(bookingId, out var codes))
                {
                    codes = [];
                    codesByBookingId[bookingId] = codes;
                }

                if (!codes.Contains(conflict.Code))
                {
                    codes.Add(conflict.Code);
                }
            }
        }

        return codesByBookingId;
    }
}
