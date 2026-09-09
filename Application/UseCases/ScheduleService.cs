using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Application.Contracts;

namespace TutoringScheduling.Application;

/// <summary>
/// Read side of the schedule: runs the conflict engine over stored bookings
/// and shapes the results for the API.
/// </summary>
public sealed class ScheduleService
{
    private readonly IBookingStore _bookings;
    private readonly IRoomStore _rooms;
    private readonly ILessonEventStore _events;

    public ScheduleService(IBookingStore bookings, IRoomStore rooms, ILessonEventStore events)
    {
        _bookings = bookings;
        _rooms = rooms;
        _events = events;
    }

    public async Task<ScheduleDayResponse> GetDayAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var rooms = await _rooms.GetRoomsAsync(cancellationToken);
        var bookings = await _bookings.GetBookingsForDayAsync(date, cancellationToken);

        var conflicts = ConflictDetector.Detect(bookings);
        var codesByBookingId = MapConflictCodesByBooking(conflicts);

        var cutoffMoves = await _events.GetCutoffMovesTouchingDayAsync(date, cancellationToken);

        var landedLate = cutoffMoves
            .Where(lessonEvent => lessonEvent.ToDate == date)
            .Select(lessonEvent => lessonEvent.LessonId)
            .ToHashSet();

        var roomSchedules = rooms
            .Select(room => new RoomScheduleDto
            {
                RoomId = room.Id,
                RoomName = room.Name,
                Lessons = bookings
                    .Where(booking => booking.RoomId == room.Id)
                    .OrderBy(booking => booking.StartTime)
                    .ThenBy(booking => booking.Id, StringComparer.Ordinal)
                    .Select(booking => LessonMapper.ToDto(
                        booking,
                        codesByBookingId.GetValueOrDefault(booking.Id),
                        movedAfterCutoff: landedLate.Contains(booking.Id)))
                    .ToList(),
            })
            .ToList();

        return new ScheduleDayResponse
        {
            Date = date,
            IsMonday = date.DayOfWeek == DayOfWeek.Monday,
            Rooms = roomSchedules,
            Conflicts = conflicts.ToList(),
            Changes = cutoffMoves
                .OrderBy(lessonEvent => lessonEvent.OccurredAt)
                .Select(LessonMapper.ToDto)
                .ToList(),
        };
    }

    public async Task<LessonHistoryResponse?> GetLessonHistoryAsync(
        string lessonId,
        CancellationToken cancellationToken = default)
    {
        var lesson = await _bookings.FindBookingAsync(lessonId, cancellationToken);

        if (lesson is null)
        {
            return null;
        }

        // SQLite cannot sort by DateTimeOffset, so order once materialised.
        var events = await _events.GetLessonEventsAsync(lessonId, cancellationToken);

        return new LessonHistoryResponse
        {
            Lesson = LessonMapper.ToDto(lesson),
            Events = events
                .OrderBy(lessonEvent => lessonEvent.OccurredAt)
                .ThenBy(lessonEvent => lessonEvent.Id)
                .Select(LessonMapper.ToDto)
                .ToList(),
        };
    }

    public async Task<ConflictsResponse> GetConflictsAsync(
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var bookings = await _bookings.GetBookingsInRangeAsync(from, to, cancellationToken);
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
