using TutoringScheduling.Api.Extensions;
using TutoringScheduling.Application;

namespace TutoringScheduling.Api.Endpoints.Lessons;

public static class LessonHistoryEndpoint
{
    public static IEndpointRouteBuilder MapLessonHistoryEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/lessons/{id}/history", async (
            string id,
            IScheduleService schedule,
            CancellationToken cancellationToken) =>
        {
            var result = await schedule.GetLessonHistoryAsync(id, cancellationToken);
            return result.ToHttpResult();
        });

        return endpoints;
    }
}
