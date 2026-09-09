using TutoringScheduling.Domain;
using TutoringScheduling.Application;
using TutoringScheduling.Tests.Support;

namespace TutoringScheduling.Tests;

public class TutorServiceTests : SqlServerFixture
{
    private TutorService Service => NewTutorService();

    [Fact]
    public async Task GetLoadsForDayAsync_WithNoBookings_ReturnsEmptyLoadsForThatDate()
    {
        var loads = await Service.GetLoadsForDayAsync(new DateOnly(2026, 3, 6));

        Assert.Equal(new DateOnly(2026, 3, 6), loads.Date);
        Assert.Empty(loads.TutorLoads);
    }

    [Fact]
    public async Task GetLoadsForDayAsync_CountsBookedLessonsAndReportsTheLimit()
    {
        Add(
            BookingFactory.Create("A", start: "08:00", roomId: "R1"),
            BookingFactory.Create("B", start: "10:00", roomId: "R2"));

        var load = Assert.Single((await Service.GetLoadsForDayAsync(new DateOnly(2026, 3, 6))).TutorLoads);

        Assert.Equal("T1", load.TutorId);
        Assert.Equal(2, load.LessonCount);
        Assert.Equal(BookingLimits.MaxBookingsPerTutorPerDay, load.Limit);
        Assert.False(load.OverLimit);
    }

    [Fact]
    public async Task GetLoadsForDayAsync_FlagsATutorOverTheDailyLimit()
    {
        Add(Enumerable.Range(1, 7)
            .Select(hour => BookingFactory.Create(
                $"L{hour}", start: $"{7 + hour:00}:00", roomId: $"R{(hour % 6) + 1}"))
            .ToArray());

        var load = Assert.Single((await Service.GetLoadsForDayAsync(new DateOnly(2026, 3, 6))).TutorLoads);

        Assert.Equal(7, load.LessonCount);
        Assert.True(load.OverLimit);
    }

    [Fact]
    public async Task GetLoadsForDayAsync_IgnoresCancelledLessons()
    {
        Add(
            BookingFactory.Create("A", start: "08:00", roomId: "R1"),
            BookingFactory.Create("B", start: "10:00", roomId: "R2", status: BookingStatus.Cancelled));

        var load = Assert.Single((await Service.GetLoadsForDayAsync(new DateOnly(2026, 3, 6))).TutorLoads);

        Assert.Equal(1, load.LessonCount);
    }
}
