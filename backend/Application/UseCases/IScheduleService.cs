using TutoringScheduling.Application.Common;
using TutoringScheduling.Application.Contracts;

namespace TutoringScheduling.Application;

/// <summary>
/// Read side of the schedule: shapes the day's board and, separately, the
/// conflict engine's results over stored bookings.
/// </summary>
public interface IScheduleService
{
    /// <summary>The board's data for one day: rooms (with per-lesson conflict codes) and post-cut-off changes.</summary>
    Task<Result<ScheduleDayResponse>> GetDayAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>A lesson's current state plus its event log; a NotFound error if the lesson is unknown.</summary>
    Task<Result<LessonHistoryResponse>> GetLessonHistoryAsync(
        string lessonId, CancellationToken cancellationToken = default);

    /// <summary>Every clash over stored bookings within the (optional) range, plus a count.</summary>
    Task<Result<ConflictsResponse>> GetConflictsAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}
