using TutoringScheduling.Domain;

namespace TutoringScheduling.Tests;

internal static class BookingFactory
{
    // 2026-03-06 is a Friday, so the default does not trip the Monday rule.
    public static Booking Create(
        string id,
        string date = "2026-03-06",
        string start = "09:00",
        int durationMinutes = 60,
        string tutorId = "T1",
        string roomId = "R1",
        string student = "Student A",
        BookingStatus status = BookingStatus.Booked,
        string? groupId = null)
    {
        return new Booking
        {
            Id = id,
            LessonDate = DateOnly.Parse(date),
            StartTime = TimeOnly.Parse(start),
            DurationMinutes = durationMinutes,
            TutorId = tutorId,
            RoomId = roomId,
            StudentName = student,
            Status = status,
            GroupId = groupId,
        };
    }
}
