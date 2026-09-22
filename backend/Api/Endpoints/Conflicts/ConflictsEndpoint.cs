using TutoringScheduling.Api.Extensions;
using TutoringScheduling.Application;

namespace TutoringScheduling.Api.Endpoints.Conflicts;

public static class ConflictsEndpoint
{
    public static IEndpointRouteBuilder MapConflictsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/conflicts", async (
            DateOnly? from,
            DateOnly? to,
            IScheduleService schedule,
            CancellationToken cancellationToken) =>
        {
            var result = await schedule.GetConflictsAsync(from, to, cancellationToken);
            return result.ToHttpResult();
        });

        return endpoints;
    }
}
