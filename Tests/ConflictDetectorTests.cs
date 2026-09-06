using TutoringScheduling.Contracts;
using TutoringScheduling.Domain;
using TutoringScheduling.Services;

namespace TutoringScheduling.Tests;

public class ConflictDetectorTests
{
    [Fact]
    public void ExamPair_SharedGroup_ProducesNoConflict()
    {
        var bookings = new[]
        {
            BookingFactory.Create("A", start: "11:00", durationMinutes: 90, student: "Ha", groupId: "G1"),
            BookingFactory.Create("B", start: "11:00", durationMinutes: 90, student: "Long", groupId: "G1"),
        };

        Assert.Empty(ConflictDetector.Detect(bookings));
    }

    [Fact]
    public void ExamPair_StillClashesWithAnUnrelatedThirdLesson()
    {
        var bookings = new[]
        {
            BookingFactory.Create("A", start: "11:00", durationMinutes: 90, student: "Ha", groupId: "G1"),
            BookingFactory.Create("B", start: "11:00", durationMinutes: 90, student: "Long", groupId: "G1"),
            BookingFactory.Create("C", start: "11:30", roomId: "R1", tutorId: "T9", student: "Kien"),
        };

        var conflicts = ConflictDetector.Detect(bookings);

        Assert.Contains(conflicts, c => c.Code == ConflictCodes.RoomDoubleBooked && c.BookingIds.Contains("C"));
    }

    [Fact]
    public void SameTutor_TwoRooms_SameSlot_FlagsTutorDoubleBookedOnly()
    {
        var bookings = new[]
        {
            BookingFactory.Create("L033", tutorId: "T1", roomId: "R1", student: "Chau"),
            BookingFactory.Create("L034", tutorId: "T1", roomId: "R2", student: "Long"),
        };

        var conflicts = ConflictDetector.Detect(bookings);

        var conflict = Assert.Single(conflicts);
        Assert.Equal(ConflictCodes.TutorDoubleBooked, conflict.Code);
        Assert.Equal(ConflictSeverities.Error, conflict.Severity);
        Assert.Equal(new[] { "L033", "L034" }, conflict.BookingIds);
        Assert.Equal("T1", conflict.TutorId);
    }

    [Fact]
    public void SameRoom_TwoTutors_FlagsRoomDoubleBooked()
    {
        var bookings = new[]
        {
            BookingFactory.Create("A", tutorId: "T1", roomId: "R1", student: "Chau"),
            BookingFactory.Create("B", tutorId: "T2", roomId: "R1", student: "Long"),
        };

        var conflicts = ConflictDetector.Detect(bookings);

        var conflict = Assert.Single(conflicts);
        Assert.Equal(ConflictCodes.RoomDoubleBooked, conflict.Code);
        Assert.Equal("R1", conflict.RoomId);
    }

    [Fact]
    public void SameStudent_TwoTutors_FlagsStudentDoubleBooked()
    {
        var bookings = new[]
        {
            BookingFactory.Create("L007", tutorId: "T3", roomId: "R3", student: "Le Minh Chau"),
            BookingFactory.Create("L008", tutorId: "T2", roomId: "R2", student: "Le Minh Chau"),
        };

        var conflict = Assert.Single(ConflictDetector.Detect(bookings));
        Assert.Equal(ConflictCodes.StudentDoubleBooked, conflict.Code);
        Assert.Equal("Le Minh Chau", conflict.StudentName);
    }

    [Fact]
    public void CancelledBooking_DoesNotClash()
    {
        var bookings = new[]
        {
            BookingFactory.Create("A", tutorId: "T1"),
            BookingFactory.Create("B", tutorId: "T1", student: "Long", status: BookingStatus.Cancelled),
        };

        Assert.Empty(ConflictDetector.Detect(bookings));
    }

    [Fact]
    public void NoShowBooking_StillClashes()
    {
        var bookings = new[]
        {
            BookingFactory.Create("A", tutorId: "T1"),
            BookingFactory.Create("B", tutorId: "T1", student: "Long", status: BookingStatus.NoShow),
        };

        Assert.Contains(ConflictDetector.Detect(bookings), c => c.Code == ConflictCodes.TutorDoubleBooked);
    }

    [Fact]
    public void BackToBackLessons_DoNotOverlap()
    {
        var bookings = new[]
        {
            BookingFactory.Create("A", start: "09:00", durationMinutes: 60, tutorId: "T1"),
            BookingFactory.Create("B", start: "10:00", durationMinutes: 60, tutorId: "T1", student: "Long"),
        };

        Assert.Empty(ConflictDetector.Detect(bookings));
    }

    [Fact]
    public void TutorWithSevenLessonsInADay_FlagsDailyLimitWarning()
    {
        var bookings = Enumerable.Range(0, 7)
            .Select(i => BookingFactory.Create(
                $"L{i}",
                start: TimeOnly.Parse("08:00").AddHours(i).ToString("HH:mm"),
                tutorId: "T1",
                roomId: $"R{i}",
                student: $"S{i}"))
            .ToArray();

        var conflict = Assert.Single(ConflictDetector.Detect(bookings));
        Assert.Equal(ConflictCodes.TutorDailyLimit, conflict.Code);
        Assert.Equal(ConflictSeverities.Warning, conflict.Severity);
        Assert.Equal(7, conflict.BookingIds.Count);
        Assert.Equal("T1", conflict.TutorId);
    }

    [Fact]
    public void TutorWithSixLessonsInADay_IsWithinLimit()
    {
        var bookings = Enumerable.Range(0, 6)
            .Select(i => BookingFactory.Create(
                $"L{i}",
                start: TimeOnly.Parse("08:00").AddHours(i).ToString("HH:mm"),
                tutorId: "T1",
                roomId: $"R{i}",
                student: $"S{i}"))
            .ToArray();

        Assert.Empty(ConflictDetector.Detect(bookings));
    }

    [Fact]
    public void MondayLesson_FlagsCentreClosedWarning()
    {
        var bookings = new[]
        {
            BookingFactory.Create("L032", date: "2026-03-09"),
        };

        var conflict = Assert.Single(ConflictDetector.Detect(bookings));
        Assert.Equal(ConflictCodes.CentreClosedMonday, conflict.Code);
        Assert.Equal(ConflictSeverities.Warning, conflict.Severity);
        Assert.Contains("L032", conflict.BookingIds);
    }

    [Fact]
    public void Errors_AreOrderedBeforeWarnings()
    {
        var bookings = new[]
        {
            BookingFactory.Create("M", date: "2026-03-09"),
            BookingFactory.Create("A", date: "2026-03-10", tutorId: "T1", roomId: "R1", student: "Chau"),
            BookingFactory.Create("B", date: "2026-03-10", tutorId: "T1", roomId: "R2", student: "Long"),
        };

        var conflicts = ConflictDetector.Detect(bookings);

        Assert.Equal(ConflictSeverities.Error, conflicts[0].Severity);
        Assert.Equal(ConflictSeverities.Warning, conflicts[^1].Severity);
    }
}
