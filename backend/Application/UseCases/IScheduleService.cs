using TutoringScheduling.Application.Contracts;

namespace TutoringScheduling.Application;

/// <summary>
/// Read side of the schedule: runs the conflict engine over stored bookings
/// and shapes the results for the API.
/// </summary>
public interface IScheduleService
{
    /// <summary>The board's data for one day: rooms, conflicts, post-cut-off changes.</summary>
    Task<ScheduleDayResponse> GetDayAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>A lesson's current state plus its event log, or null if the lesson is unknown.</summary>
    Task<LessonHistoryResponse?> GetLessonHistoryAsync(
        string lessonId, CancellationToken cancellationToken = default);

    /// <summary>Every clash over stored bookings within the (optional) range, plus a count.</summary>
    Task<ConflictsResponse> GetConflictsAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}
