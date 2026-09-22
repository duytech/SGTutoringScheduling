using TutoringScheduling.Api.Extensions;
using TutoringScheduling.Application;

namespace TutoringScheduling.Api.Endpoints.Schedule;

public static class ScheduleEndpoint
{
    public static IEndpointRouteBuilder MapScheduleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/schedule", async (
            DateOnly date,
            IScheduleService schedule,
            CancellationToken cancellationToken) =>
        {
            var result = await schedule.GetDayAsync(date, cancellationToken);
            return result.ToHttpResult();
        });

        return endpoints;
    }
}
