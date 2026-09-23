using TutoringScheduling.Application.Contracts;
using TutoringScheduling.Domain;
using TutoringScheduling.Tests.Support;

namespace TutoringScheduling.Tests;

public class LessonHistoryTests : SqlServerFixture
{
    [Fact]
    public async Task History_ListsSeededAndAppliedEventsOldestFirst()
    {
        Add(BookingFactory.Create("L1", date: "2026-03-10", start: "09:00", roomId: "R1"));
        Db.LessonEvents.Add(new LessonEvent
        {
            LessonId = "L1",
            Type = LessonEventType.Created,
            OccurredAt = new DateTimeOffset(new DateOnly(2026, 3, 3), new TimeOnly(12, 0), CentreCalendar.TimeZoneOffset),
        });
        Db.SaveChanges();
        Db.ChangeTracker.Clear();

        await NewMover(new FixedClock("2026-03-06T09:00:00"))
            .MoveAsync("L1", new() { ToDate = new DateOnly(2026, 3, 10), ToStartTime = new TimeOnly(11, 0) });

        var result = await NewScheduleService().GetLessonHistoryAsync("L1");

        Assert.True(result.IsSuccess);
        var history = result.Value!;
        Assert.Equal(new[] { "Created", "Moved" }, history.Events.Select(e => e.Type));
        Assert.Equal(new TimeOnly(11, 0), history.Lesson.StartTime);
    }

    [Fact]
    public async Task History_ForAnUnknownLesson_IsNotFound()
    {
        var result = await NewScheduleService().GetLessonHistoryAsync("nope");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.LessonNotFound, result.Error!.Code);
    }
}
