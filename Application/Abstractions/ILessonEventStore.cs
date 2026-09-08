using TutoringScheduling.Domain;

namespace TutoringScheduling.Application.Abstractions;

/// <summary>
/// Read access to the append-only lesson event log. Infrastructure supplies the
/// EF Core implementation; the application never sees a <c>DbContext</c>.
/// </summary>
public interface ILessonEventStore
{
    /// <summary>Every event recorded against a lesson, in no particular order.</summary>
    Task<IReadOnlyList<LessonEvent>> GetLessonEventsAsync(
        string lessonId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <see cref="LessonEventType.Moved"/> events made past the day-before cut-off
    /// that either left <paramref name="date"/> or landed on it.
    /// </summary>
    Task<IReadOnlyList<LessonEvent>> GetCutoffMovesTouchingDayAsync(
        DateOnly date, CancellationToken cancellationToken = default);
}
