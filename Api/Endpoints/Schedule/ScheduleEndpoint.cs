using TutoringScheduling.Application;

namespace TutoringScheduling.Api.Endpoints.Schedule;

public static class ScheduleEndpoint
{
    public static IEndpointRouteBuilder MapScheduleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/schedule", async (
            DateOnly date,
            ScheduleService schedule,
            CancellationToken cancellationToken) =>
        {
            var response = await schedule.GetDayAsync(date, cancellationToken);
            return Results.Ok(response);
        });

        return endpoints;
    }
}
