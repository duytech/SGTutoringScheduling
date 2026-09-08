using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="ILessonEventStore"/>. One of the few
/// places a <see cref="AppDbContext"/> is touched on the read path.
/// </summary>
public sealed class LessonEventStore : ILessonEventStore
{
    private readonly AppDbContext _db;

    public LessonEventStore(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LessonEvent>> GetLessonEventsAsync(
        string lessonId, CancellationToken cancellationToken = default)
    {
        return await _db.LessonEvents
            .Where(lessonEvent => lessonEvent.LessonId == lessonId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LessonEvent>> GetCutoffMovesTouchingDayAsync(
        DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _db.LessonEvents
            .Where(lessonEvent => lessonEvent.Type == LessonEventType.Moved && lessonEvent.AfterCutoff)
            .Where(lessonEvent => lessonEvent.ToDate == date || lessonEvent.FromDate == date)
            .ToListAsync(cancellationToken);
    }
}
