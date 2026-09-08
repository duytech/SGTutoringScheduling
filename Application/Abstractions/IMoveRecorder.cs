using TutoringScheduling.Domain;

namespace TutoringScheduling.Application.Abstractions;

/// <summary>
/// The write side of a reschedule. Infrastructure supplies the EF Core
/// implementation; the application never sees a <c>DbContext</c>.
/// </summary>
public interface IMoveRecorder
{
    /// <summary>
    /// Persists the rescheduled lesson and appends its move event as one unit of work.
    /// </summary>
    Task RecordMoveAsync(
        Booking movedLesson, LessonEvent moveEvent, CancellationToken cancellationToken = default);
}
