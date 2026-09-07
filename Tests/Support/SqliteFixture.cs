using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Composition;
using TutoringScheduling.Data;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Tests.Support;

/// <summary>
/// A throwaway SQLite database (real schema, in memory) seeded with six rooms
/// and three tutors. One instance per test.
/// </summary>
public abstract class SqliteFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    protected SqliteFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        Db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options);
        Db.Database.EnsureCreated();

        Db.Rooms.AddRange(Enumerable.Range(1, 6)
            .Select(n => new Room { Id = $"R{n}", Name = $"Room {n}" }));
        Db.Tutors.AddRange(
            new Tutor { Id = "T1", Name = "Ngoc Anh", Subject = "Maths", Phone = "x" },
            new Tutor { Id = "T2", Name = "Pham Duc", Subject = "English", Phone = "x" },
            new Tutor { Id = "T3", Name = "Le Thu", Subject = "Physics", Phone = "x" });
        Db.SaveChanges();
    }

    protected AppDbContext Db { get; }

    /// <summary>The persistence port backed by this fixture's database.</summary>
    protected IScheduleStore Store => new ScheduleStore(Db);

    protected void Add(params Booking[] bookings)
    {
        Db.Bookings.AddRange(bookings);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
