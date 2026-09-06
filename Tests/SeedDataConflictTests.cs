using SynergieGlobalTutoringScheduling.Contracts;
using SynergieGlobalTutoringScheduling.Data;
using SynergieGlobalTutoringScheduling.Services;

namespace SynergieGlobalTutoringScheduling.Tests;

/// <summary>
/// Runs the detector over the real seed export and pins down every conflict
/// the reviewer notes call out (brief-notes.md).
/// </summary>
public class SeedDataConflictTests
{
    private static IReadOnlyList<ConflictDto> DetectSeedConflicts()
    {
        var csvPath = Path.Combine(AppContext.BaseDirectory, "seed-data", "lessons_export.csv");
        var bookings = DbSeeder.ReadBookings(csvPath);
        return ConflictDetector.Detect(bookings);
    }

    [Fact]
    public void SeedData_HasTwoErrorsAndTwoWarnings()
    {
        var conflicts = DetectSeedConflicts();

        Assert.Equal(2, conflicts.Count(c => c.Severity == ValidationIssueSeverities.Error));
        Assert.Equal(2, conflicts.Count(c => c.Severity == ValidationIssueSeverities.Warning));
    }

    [Fact]
    public void SeedData_FlagsTheTutorDoubleBooking()
    {
        var conflict = Assert.Single(
            DetectSeedConflicts(),
            c => c.Code == ConflictCodes.TutorDoubleBooked);

        Assert.Equal(new[] { "L033", "L034" }, conflict.BookingIds);
        Assert.Equal("T1", conflict.TutorId);
        Assert.Equal(new DateOnly(2026, 3, 10), conflict.Date);
    }

    [Fact]
    public void SeedData_FlagsTheStudentBookedWithTwoTutors()
    {
        var conflict = Assert.Single(
            DetectSeedConflicts(),
            c => c.Code == ConflictCodes.StudentDoubleBooked);

        Assert.Equal(new[] { "L007", "L008" }, conflict.BookingIds);
        Assert.Equal("Le Minh Chau", conflict.StudentName);
    }

    [Fact]
    public void SeedData_DoesNotFlagTheExamPair()
    {
        var conflicts = DetectSeedConflicts();

        Assert.DoesNotContain(conflicts, c => c.BookingIds.Contains("L009") || c.BookingIds.Contains("L010"));
        Assert.DoesNotContain(conflicts, c => c.Code == ConflictCodes.RoomDoubleBooked);
    }

    [Fact]
    public void SeedData_FlagsTutorT1OverTheDailyLimitOnMarch6()
    {
        var conflict = Assert.Single(
            DetectSeedConflicts(),
            c => c.Code == ConflictCodes.TutorDailyLimit);

        Assert.Equal("T1", conflict.TutorId);
        Assert.Equal(new DateOnly(2026, 3, 6), conflict.Date);
        Assert.Equal(
            new[] { "L018", "L021", "L022", "L024", "L025", "L026", "L027" },
            conflict.BookingIds);
    }

    [Fact]
    public void SeedData_FlagsTheMondayLesson()
    {
        var conflict = Assert.Single(
            DetectSeedConflicts(),
            c => c.Code == ConflictCodes.CentreClosedMonday);

        Assert.Equal(new DateOnly(2026, 3, 9), conflict.Date);
        Assert.Equal(new[] { "L032" }, conflict.BookingIds);
    }
}
