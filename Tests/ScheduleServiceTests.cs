using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SynergieGlobalTutoringScheduling.Contracts;
using SynergieGlobalTutoringScheduling.Data;
using SynergieGlobalTutoringScheduling.Domain;
using SynergieGlobalTutoringScheduling.Services;

namespace SynergieGlobalTutoringScheduling.Tests;

public class ScheduleServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;

    public ScheduleServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _db.Rooms.AddRange(
            Enumerable.Range(1, 6).Select(n => new Room { Id = $"R{n}", Name = $"Room {n}" }));
        _db.Tutors.Add(new Tutor { Id = "T1", Name = "Ngoc Anh", Subject = "Maths", Phone = "x" });
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetDayAsync_ReturnsEveryRoom_EvenWhenEmpty()
    {
        var day = await new ScheduleService(_db).GetDayAsync(new DateOnly(2026, 3, 6));

        Assert.Equal(6, day.Rooms.Count);
        Assert.All(day.Rooms, room => Assert.Empty(room.Lessons));
        Assert.False(day.IsMonday);
    }

    [Fact]
    public async Task GetDayAsync_TagsEachLessonInvolvedInAClash()
    {
        _db.Bookings.AddRange(
            Booking("A", "R1", "09:00", student: "Chau"),
            Booking("B", "R2", "09:00", student: "Long", tutorId: "T1"));
        // Same tutor, two rooms, same slot -> tutor double booked.
        _db.SaveChanges();

        var day = await new ScheduleService(_db).GetDayAsync(new DateOnly(2026, 3, 6));

        var lessons = day.Rooms.SelectMany(r => r.Lessons).ToList();
        Assert.All(lessons, lesson =>
            Assert.Contains(ConflictCodes.TutorDoubleBooked, lesson.ConflictCodes));
        Assert.Single(day.Conflicts, c => c.Code == ConflictCodes.TutorDoubleBooked);
    }

    [Fact]
    public async Task GetConflictsAsync_RespectsDateRange()
    {
        _db.Bookings.AddRange(
            Booking("M", "R1", "10:00", date: "2026-03-09"));
        _db.SaveChanges();

        var service = new ScheduleService(_db);

        Assert.Empty((await service.GetConflictsAsync(
            new DateOnly(2026, 3, 6), new DateOnly(2026, 3, 8))).Conflicts);
        Assert.Single((await service.GetConflictsAsync(
            new DateOnly(2026, 3, 9), new DateOnly(2026, 3, 9))).Conflicts);
    }

    private static Booking Booking(
        string id,
        string roomId,
        string start,
        string date = "2026-03-06",
        string tutorId = "T1",
        string student = "Student A")
    {
        return new Booking
        {
            Id = id,
            LessonDate = DateOnly.Parse(date),
            StartTime = TimeOnly.Parse(start),
            DurationMinutes = 60,
            TutorId = tutorId,
            RoomId = roomId,
            StudentName = student,
            Status = BookingStatus.Booked,
        };
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
