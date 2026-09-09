using TutoringScheduling.Application.Contracts;

namespace TutoringScheduling.Application;

/// <summary>
/// Read side of the tutors resource: per-tutor lesson counts for a day and the
/// over-the-6-limit flag.
/// </summary>
public interface ITutorService
{
    Task<TutorLoadsResponse> GetLoadsForDayAsync(
        DateOnly date, CancellationToken cancellationToken = default);
}
