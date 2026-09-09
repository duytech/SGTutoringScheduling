using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IMoveRecorder"/>. The only place a
/// <see cref="AppDbContext"/> is touched on the write path.
/// </summary>
public sealed class MoveRecorder : IMoveRecorder
{
    private readonly AppDbContext _db;

    public MoveRecorder(AppDbContext db)
    {
        _db = db;
    }

    public async Task RecordMoveAsync(
        Booking movedLesson, LessonEvent moveEvent, CancellationToken cancellationToken = default)
    {
        _db.Bookings.Update(movedLesson);
        _db.LessonEvents.Add(moveEvent);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
