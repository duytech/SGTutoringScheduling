using Microsoft.EntityFrameworkCore;
using SynergieGlobalTutoringScheduling.Contracts;
using SynergieGlobalTutoringScheduling.Data;

namespace SynergieGlobalTutoringScheduling.Services;

/// <summary>
/// Read side of the schedule: runs the conflict engine over stored bookings
/// and shapes the results for the API.
/// </summary>
public sealed class ScheduleService
{
    private readonly AppDbContext _db;

    public ScheduleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ConflictsResponse> GetConflictsAsync(
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Bookings.AsQueryable();

        if (from is { } fromDate)
        {
            query = query.Where(booking => booking.LessonDate >= fromDate);
        }

        if (to is { } toDate)
        {
            query = query.Where(booking => booking.LessonDate <= toDate);
        }

        var bookings = await query.ToListAsync(cancellationToken);
        var conflicts = ConflictDetector.Detect(bookings);

        return new ConflictsResponse
        {
            From = from,
            To = to,
            Summary = new ConflictSummary
            {
                Errors = conflicts.Count(c => c.Severity == ValidationIssueSeverities.Error),
                Warnings = conflicts.Count(c => c.Severity == ValidationIssueSeverities.Warning),
            },
            Conflicts = conflicts.ToList(),
        };
    }
}
