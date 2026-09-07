using TutoringScheduling.Application.Contracts;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Application;

public static class LessonMapper
{
    public static LessonDto ToDto(
        Booking booking,
        IReadOnlyList<string>? conflictCodes = null,
        bool movedAfterCutoff = false)
    {
        return new LessonDto
        {
            Id = booking.Id,
            Date = booking.LessonDate,
            StartTime = booking.StartTime,
            EndTime = booking.StartTime.AddMinutes(booking.DurationMinutes),
            DurationMinutes = booking.DurationMinutes,
            StudentName = booking.StudentName,
            TutorId = booking.TutorId,
            TutorName = booking.Tutor.Name,
            RoomId = booking.RoomId,
            Status = booking.Status.ToString(),
            GroupId = booking.GroupId,
            MovedAfterCutoff = movedAfterCutoff,
            ConflictCodes = conflictCodes is null ? [] : [.. conflictCodes],
        };
    }

    public static LessonEventDto ToDto(LessonEvent lessonEvent)
    {
        return new LessonEventDto
        {
            LessonId = lessonEvent.LessonId,
            Type = lessonEvent.Type.ToString(),
            OccurredAt = lessonEvent.OccurredAt,
            FromDate = lessonEvent.FromDate,
            FromStartTime = lessonEvent.FromStartTime,
            FromRoomId = lessonEvent.FromRoomId,
            ToDate = lessonEvent.ToDate,
            ToStartTime = lessonEvent.ToStartTime,
            ToRoomId = lessonEvent.ToRoomId,
            Reason = lessonEvent.Reason,
            AfterCutoff = lessonEvent.AfterCutoff,
        };
    }
}
