using TutoringScheduling.Domain;
using TutoringScheduling.Services;
using TutoringScheduling.Tests.Support;

namespace TutoringScheduling.Tests;

public class LessonHistoryTests : SqliteFixture
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

        await new MoveLessonService(Db, new FixedClock("2026-03-06T09:00:00"))
            .MoveAsync("L1", new() { ToDate = new DateOnly(2026, 3, 10), ToStartTime = new TimeOnly(11, 0) });

        var history = await new ScheduleService(Db).GetLessonHistoryAsync("L1");

        Assert.NotNull(history);
        Assert.Equal(new[] { "Created", "Moved" }, history!.Events.Select(e => e.Type));
        Assert.Equal(new TimeOnly(11, 0), history.Lesson.StartTime);
    }

    [Fact]
    public async Task History_ForAnUnknownLesson_IsNull()
    {
        Assert.Null(await new ScheduleService(Db).GetLessonHistoryAsync("nope"));
    }
}
