using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Application;
using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Application.Contracts;
using TutoringScheduling.Data;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Services;

public enum MoveOutcome
{
    Applied,
    LessonNotFound,
    Rejected,
}

public sealed class MoveResult
{
    public required MoveOutcome Outcome { get; init; }

    public MoveLessonResponse? Applied { get; init; }

    public string? RejectionReason { get; init; }

    public IReadOnlyList<ConflictDto> Conflicts { get; init; } = [];

    public static MoveResult NotFound() => new() { Outcome = MoveOutcome.LessonNotFound };

    public static MoveResult Reject(string reason, IReadOnlyList<ConflictDto>? conflicts = null) =>
        new() { Outcome = MoveOutcome.Rejected, RejectionReason = reason, Conflicts = conflicts ?? [] };

    public static MoveResult Ok(MoveLessonResponse response) =>
        new() { Outcome = MoveOutcome.Applied, Applied = response };
}

/// <summary>
/// Reschedules a single lesson. Runs the one conflict engine against the
/// target slot, applies the change to the current-state row, and records it
/// as an event so a move made after the cut-off stays visible as a change.
/// </summary>
public sealed class MoveLessonService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;

    public MoveLessonService(AppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<MoveResult> MoveAsync(
        string lessonId,
        MoveLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ToDate == default || request.ToStartTime == default)
        {
            return MoveResult.Reject("toDate and toStartTime are required.");
        }

        var lesson = await _db.Bookings
            .Include(booking => booking.Tutor)
            .FirstOrDefaultAsync(booking => booking.Id == lessonId, cancellationToken);

        if (lesson is null)
        {
            return MoveResult.NotFound();
        }

        if (lesson.Status != BookingStatus.Booked)
        {
            return MoveResult.Reject(
                $"Lesson {lessonId} is {lesson.Status.ToString().ToLowerInvariant()} and cannot be moved.");
        }

        if (lesson.GroupId is not null)
        {
            return MoveResult.Reject(
                $"Lesson {lessonId} is part of exam pair '{lesson.GroupId}'; the pair must be moved together (not supported).");
        }

        var toRoomId = request.ToRoomId ?? lesson.RoomId;

        if (request.ToDate == lesson.LessonDate
            && request.ToStartTime == lesson.StartTime
            && toRoomId == lesson.RoomId)
        {
            return MoveResult.Reject("The lesson is already at that date, time and room.");
        }

        if (CentreCalendar.IsClosed(request.ToDate))
        {
            return MoveResult.Reject($"The centre is closed on {request.ToDate:dddd dd MMM}; pick another day.");
        }

        if (!await _db.Rooms.AnyAsync(room => room.Id == toRoomId, cancellationToken))
        {
            return MoveResult.Reject($"Room '{toRoomId}' does not exist.");
        }

        var targetDay = await _db.Bookings
            .Where(booking => booking.LessonDate == request.ToDate && booking.Id != lessonId)
            .ToListAsync(cancellationToken);

        var proposed = CloneInto(lesson, request.ToDate, request.ToStartTime, toRoomId);
        targetDay.Add(proposed);

        var conflicts = ConflictDetector.Detect(targetDay)
            .Where(conflict => conflict.BookingIds.Contains(lessonId))
            .ToList();

        var blocking = conflicts.Where(c => c.Severity == ConflictSeverities.Error).ToList();
        if (blocking.Count > 0)
        {
            return MoveResult.Reject("The move would clash with another lesson.", blocking);
        }

        var from = (lesson.LessonDate, lesson.StartTime, lesson.RoomId);

        lesson.LessonDate = request.ToDate;
        lesson.StartTime = request.ToStartTime;
        lesson.RoomId = toRoomId;

        var lessonEvent = new LessonEvent
        {
            LessonId = lessonId,
            Type = LessonEventType.Moved,
            OccurredAt = _clock.Now,
            FromDate = from.LessonDate,
            FromStartTime = from.StartTime,
            FromRoomId = from.RoomId,
            ToDate = request.ToDate,
            ToStartTime = request.ToStartTime,
            ToRoomId = toRoomId,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            AfterCutoff = _clock.Now > CentreCalendar.ChangeCutoff(from.LessonDate),
        };

        _db.LessonEvents.Add(lessonEvent);
        await _db.SaveChangesAsync(cancellationToken);

        return MoveResult.Ok(new MoveLessonResponse
        {
            Lesson = LessonMapper.ToDto(lesson),
            Event = LessonMapper.ToDto(lessonEvent),
            Warnings = conflicts.Where(c => c.Severity == ConflictSeverities.Warning).ToList(),
        });
    }

    private static Booking CloneInto(Booking source, DateOnly date, TimeOnly start, string roomId)
    {
        return new Booking
        {
            Id = source.Id,
            StudentName = source.StudentName,
            LessonDate = date,
            StartTime = start,
            DurationMinutes = source.DurationMinutes,
            TutorId = source.TutorId,
            RoomId = roomId,
            Status = source.Status,
            GroupId = source.GroupId,
        };
    }
}
