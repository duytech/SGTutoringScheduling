using TutoringScheduling.Services;

namespace TutoringScheduling.Endpoints.Conflicts;

public static class ConflictsEndpoint
{
    public static IEndpointRouteBuilder MapConflictsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/conflicts", async (
            DateOnly? from,
            DateOnly? to,
            ScheduleService schedule,
            CancellationToken cancellationToken) =>
        {
            var response = await schedule.GetConflictsAsync(from, to, cancellationToken);
            return Results.Ok(response);
        });

        return endpoints;
    }
}
