using TutoringScheduling.Application.Contracts;
using TutoringScheduling.Application;
using TutoringScheduling.Tests.Support;

namespace TutoringScheduling.Tests;

public class ScheduleServiceTests : SqlServerFixture
{
    private ScheduleService Service => NewScheduleService();

    [Fact]
    public async Task GetDayAsync_ReturnsEveryRoom_EvenWhenEmpty()
    {
        var day = await Service.GetDayAsync(new DateOnly(2026, 3, 6));

        Assert.Equal(6, day.Rooms.Count);
        Assert.All(day.Rooms, room => Assert.Empty(room.Lessons));
        Assert.False(day.IsMonday);
    }

    [Fact]
    public async Task GetDayAsync_TagsEachLessonInvolvedInAClash()
    {
        // Same tutor, two rooms, same slot -> tutor double booked.
        Add(
            BookingFactory.Create("A", roomId: "R1", student: "Chau"),
            BookingFactory.Create("B", roomId: "R2", student: "Long"));

        var day = await Service.GetDayAsync(new DateOnly(2026, 3, 6));

        var lessons = day.Rooms.SelectMany(room => room.Lessons).ToList();
        Assert.Equal(2, lessons.Count);
        Assert.All(lessons, lesson =>
            Assert.Contains(ConflictCodes.TutorDoubleBooked, lesson.ConflictCodes));
        Assert.Single(day.Conflicts, c => c.Code == ConflictCodes.TutorDoubleBooked);
    }

    [Fact]
    public async Task GetDayAsync_ListsMovesMadeAfterTheCutoffAndMarksTheLesson()
    {
        Add(BookingFactory.Create("L1", date: "2026-03-06", start: "09:00", roomId: "R1"));

        await NewMover(new FixedClock("2026-03-06T09:00:00")).MoveAsync(
            "L1",
            new() { ToDate = new DateOnly(2026, 3, 6), ToStartTime = new TimeOnly(11, 0) });

        var day = await Service.GetDayAsync(new DateOnly(2026, 3, 6));

        Assert.Single(day.Changes, change => change.LessonId == "L1");
        var lesson = day.Rooms.SelectMany(room => room.Lessons).Single();
        Assert.True(lesson.MovedAfterCutoff);
    }

    [Fact]
    public async Task GetConflictsAsync_RespectsDateRange()
    {
        Add(BookingFactory.Create("M", date: "2026-03-09", start: "10:00"));

        Assert.Empty((await Service.GetConflictsAsync(
            new DateOnly(2026, 3, 6), new DateOnly(2026, 3, 8))).Conflicts);
        Assert.Single((await Service.GetConflictsAsync(
            new DateOnly(2026, 3, 9), new DateOnly(2026, 3, 9))).Conflicts);
    }
}
