using TutoringScheduling.Application.Common;
using TutoringScheduling.Application.Contracts;

namespace TutoringScheduling.Application;

/// <summary>
/// Reschedules a single lesson: checks the target slot against the one conflict
/// engine, updates the current-state row, and records the change as an event so
/// a move made after the cut-off stays visible.
/// </summary>
public interface IMoveLessonService
{
    Task<Result<MoveLessonResponse>> MoveAsync(
        string lessonId, MoveLessonRequest request, CancellationToken cancellationToken = default);
}
