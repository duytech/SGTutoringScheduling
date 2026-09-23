using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Application.Contracts;
using TutoringScheduling.Domain;
using TutoringScheduling.Application;
using TutoringScheduling.Tests.Support;

namespace TutoringScheduling.Tests;

public class MoveLessonServiceTests : SqlServerFixture
{
    // Friday 2026-03-06, 09:00 centre time.
    private IMoveLessonService ServiceAt(string now = "2026-03-06T09:00:00") =>
        NewMover(new FixedClock(now));

    private static MoveLessonRequest To(string date, string start, string? room = null, string? reason = null) =>
        new()
        {
            ToDate = DateOnly.Parse(date),
            ToStartTime = TimeOnly.Parse(start),
            ToRoomId = room,
            Reason = reason,
        };

    [Fact]
    public async Task Move_ToAFreeSlot_UpdatesTheLessonAndRecordsAnEvent()
    {
        Add(BookingFactory.Create("L1", date: "2026-03-10", start: "09:00", roomId: "R1"));

        var result = await ServiceAt().MoveAsync("L1", To("2026-03-10", "14:00", room: "R3", reason: "family request"));

        Assert.True(result.IsSuccess);

        var lesson = await Db.Bookings.AsNoTracking().SingleAsync(b => b.Id == "L1");
        Assert.Equal(new TimeOnly(14, 0), lesson.StartTime);
        Assert.Equal("R3", lesson.RoomId);

        var moved = await Db.LessonEvents.AsNoTracking().SingleAsync(e => e.Type == LessonEventType.Moved);
        Assert.Equal("L1", moved.LessonId);
        Assert.Equal(new TimeOnly(9, 0), moved.FromStartTime);
        Assert.Equal("R1", moved.FromRoomId);
        Assert.Equal("R3", moved.ToRoomId);
        Assert.Equal("family request", moved.Reason);
    }

    [Fact]
    public async Task Move_IntoAClash_IsRejectedAndChangesNothing()
    {
        Add(
            BookingFactory.Create("L1", date: "2026-03-10", start: "09:00", roomId: "R1", tutorId: "T1", student: "Chau"),
            BookingFactory.Create("L2", date: "2026-03-10", start: "15:00", roomId: "R2", tutorId: "T1", student: "Long"));

        // Moving L1 onto 15:00 collides with L2's tutor.
        var result = await ServiceAt().MoveAsync("L1", To("2026-03-10", "15:00"));

        Assert.False(result.IsSuccess);
        var conflictError = Assert.IsType<MoveConflictError>(result.Error);
        Assert.Contains(conflictError.Conflicts, c => c.Code == ConflictCodes.TutorDoubleBooked);

        var lesson = await Db.Bookings.AsNoTracking().SingleAsync(b => b.Id == "L1");
        Assert.Equal(new TimeOnly(9, 0), lesson.StartTime);
        Assert.False(await Db.LessonEvents.AnyAsync());
    }

    [Fact]
    public async Task Move_MadeAfterTheDayBeforeCutoff_IsFlagged()
    {
        Add(BookingFactory.Create("L1", date: "2026-03-06", start: "09:00"));

        // "Now" is 2026-03-06 09:00; the cut-off for a 03-06 lesson was 03-05 16:00.
        var result = await ServiceAt().MoveAsync("L1", To("2026-03-06", "11:00"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Event.AfterCutoff);
    }

    [Fact]
    public async Task Move_MadeBeforeTheCutoff_IsNotFlagged()
    {
        Add(BookingFactory.Create("L1", date: "2026-03-10", start: "09:00"));

        var result = await ServiceAt().MoveAsync("L1", To("2026-03-10", "11:00"));

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.Event.AfterCutoff);
    }

    [Fact]
    public async Task Move_ACancelledLesson_IsRejected()
    {
        Add(BookingFactory.Create("L1", date: "2026-03-10", status: BookingStatus.Cancelled));

        var result = await ServiceAt().MoveAsync("L1", To("2026-03-11", "09:00"));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Move_APairedLesson_IsRejected()
    {
        Add(BookingFactory.Create("L1", date: "2026-03-10", groupId: "G1"));

        var result = await ServiceAt().MoveAsync("L1", To("2026-03-11", "09:00"));

        Assert.False(result.IsSuccess);
        Assert.Contains("pair", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Move_ToAMonday_IsRejected()
    {
        Add(BookingFactory.Create("L1", date: "2026-03-10"));

        var result = await ServiceAt().MoveAsync("L1", To("2026-03-09", "09:00"));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Move_AMissingLesson_ReturnsNotFound()
    {
        var result = await ServiceAt().MoveAsync("nope", To("2026-03-10", "09:00"));

        Assert.False(result.IsSuccess);
        Assert.Equal("lesson_not_found", result.Error!.Code);
    }

    [Fact]
    public async Task Move_TwoConcurrentMovesIntoTheSameFreeSlot_OnlyOneSucceeds()
    {
        Add(
            BookingFactory.Create("L1", date: "2026-03-10", start: "09:00", roomId: "R1", tutorId: "T1", student: "Chau"),
            BookingFactory.Create("L2", date: "2026-03-10", start: "10:00", roomId: "R2", tutorId: "T2", student: "Long"));

        var clock = new FixedClock("2026-03-06T09:00:00");
        var moverA = NewMoverOnOwnConnection(clock);
        var moverB = NewMoverOnOwnConnection(clock);
        var target = To("2026-03-11", "13:00", room: "R5");

        var results = await Task.WhenAll(
            moverA.MoveAsync("L1", target),
            moverB.MoveAsync("L2", target));

        Assert.Single(results, r => r.IsSuccess);
        Assert.Single(results, r => !r.IsSuccess);

        var bookedIntoSlot = await Db.Bookings.AsNoTracking()
            .Where(b => b.LessonDate == DateOnly.Parse("2026-03-11")
                && b.StartTime == TimeOnly.Parse("13:00")
                && b.RoomId == "R5")
            .ToListAsync();
        Assert.Single(bookedIntoSlot);
    }
}
