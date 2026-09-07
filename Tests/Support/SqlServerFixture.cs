using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Domain;
using TutoringScheduling.Infrastructure.Persistence;

namespace TutoringScheduling.Tests.Support;

/// <summary>
/// A throwaway SQL Server database (real schema, on localhost) seeded with six
/// rooms and three tutors. One instance per test; dropped on dispose.
/// </summary>
public abstract class SqlServerFixture : IDisposable
{
    private const string ServerConnectionString =
        "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True";

    private readonly string _databaseName = $"TutoringScheduling_Test_{Guid.NewGuid():N}";

    protected SqlServerFixture()
    {
        Db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer($"{ServerConnectionString};Database={_databaseName}")
            .Options);
        Db.Database.EnsureDeleted();
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
        Db.Database.EnsureDeleted();
        Db.Dispose();
        GC.SuppressFinalize(this);
    }
}
