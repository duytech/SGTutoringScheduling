using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Application.Common;
using TutoringScheduling.Application.Contracts;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Application;

/// <inheritdoc cref="IMoveLessonService"/>
public sealed class MoveLessonService : IMoveLessonService
{
    private readonly IBookingStore _bookings;
    private readonly IRoomStore _rooms;
    private readonly IMoveRecorder _moves;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public MoveLessonService(
        IBookingStore bookings, IRoomStore rooms, IMoveRecorder moves, IClock clock, IUnitOfWork unitOfWork)
    {
        _bookings = bookings;
        _rooms = rooms;
        _moves = moves;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MoveLessonResponse>> MoveAsync(
        string lessonId,
        MoveLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ToDate == default || request.ToStartTime == default)
        {
            return Result.Failure<MoveLessonResponse>(
                Error.Validation("move_invalid_request", "toDate and toStartTime are required."));
        }

        var lesson = await _bookings.FindBookingAsync(lessonId, cancellationToken);

        if (lesson is null)
        {
            return Result.Failure<MoveLessonResponse>(
                Error.NotFound("lesson_not_found", $"Lesson '{lessonId}' not found."));
        }

        if (lesson.Status != BookingStatus.Booked)
        {
            return Result.Failure<MoveLessonResponse>(Error.Validation(
                "move_not_bookable",
                $"Lesson {lessonId} is {lesson.Status.ToString().ToLowerInvariant()} and cannot be moved."));
        }

        if (lesson.GroupId is not null)
        {
            return Result.Failure<MoveLessonResponse>(Error.Validation(
                "move_paired_lesson",
                $"Lesson {lessonId} is part of exam pair '{lesson.GroupId}'; the pair must be moved together (not supported)."));
        }

        var toRoomId = request.ToRoomId ?? lesson.RoomId;

        if (request.ToDate == lesson.LessonDate
            && request.ToStartTime == lesson.StartTime
            && toRoomId == lesson.RoomId)
        {
            return Result.Failure<MoveLessonResponse>(
                Error.Validation("move_no_change", "The lesson is already at that date, time and room."));
        }

        if (CentreCalendar.IsClosed(request.ToDate))
        {
            return Result.Failure<MoveLessonResponse>(Error.Validation(
                "move_centre_closed",
                $"The centre is closed on {request.ToDate:dddd dd MMM}; pick another day."));
        }

        if (!await _rooms.RoomExistsAsync(toRoomId, cancellationToken))
        {
            return Result.Failure<MoveLessonResponse>(
                Error.Validation("move_room_not_found", $"Room '{toRoomId}' does not exist."));
        }

        // A Serializable transaction spans the read-check-write sequence below so
        // two concurrent moves into the same free room+slot cannot both pass the
        // conflict check and both commit (see PLAN-move-lesson-concurrency.md).
        await using var transaction = await _unitOfWork.BeginSerializableTransactionAsync(cancellationToken);

        try
        {
            var targetDay = (await _bookings.GetBookingsForDayAsync(request.ToDate, cancellationToken))
                .Where(booking => booking.Id != lessonId)
                .ToList();

            var proposed = CloneInto(lesson, request.ToDate, request.ToStartTime, toRoomId);
            targetDay.Add(proposed);

            var conflicts = ConflictDetector.Detect(targetDay)
                .Where(conflict => conflict.BookingIds.Contains(lessonId))
                .ToList();

            var blocking = conflicts.Where(c => c.Severity == ConflictSeverities.Error).ToList();
            if (blocking.Count > 0)
            {
                return Result.Failure<MoveLessonResponse>(
                    new MoveConflictError("The move would clash with another lesson.", blocking));
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

            await _moves.RecordMoveAsync(lesson, lessonEvent, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(new MoveLessonResponse
            {
                Lesson = LessonMapper.ToDto(lesson),
                Event = LessonMapper.ToDto(lessonEvent),
                Warnings = conflicts.Where(c => c.Severity == ConflictSeverities.Warning).ToList(),
            });
        }
        catch (ConcurrentWriteConflictException)
        {
            return Result.Failure<MoveLessonResponse>(Error.Conflict(
                "move_retry",
                "The slot was taken by another request at the same time; please retry."));
        }
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
