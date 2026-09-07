using TutoringScheduling.Application;
using TutoringScheduling.Application.Abstractions;

namespace TutoringScheduling.Endpoints.Schedule;

public static class ScheduleEndpoint
{
    public static IEndpointRouteBuilder MapScheduleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/schedule", async (
            DateOnly? date,
            ScheduleService schedule,
            IClock clock,
            CancellationToken cancellationToken) =>
        {
            var response = await schedule.GetDayAsync(date ?? clock.Today, cancellationToken);
            return Results.Ok(response);
        });

        return endpoints;
    }
}
