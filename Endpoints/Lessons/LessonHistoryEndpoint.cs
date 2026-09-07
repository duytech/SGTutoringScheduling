using TutoringScheduling.Application;

namespace TutoringScheduling.Endpoints.Lessons;

public static class LessonHistoryEndpoint
{
    public static IEndpointRouteBuilder MapLessonHistoryEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/lessons/{id}/history", async (
            string id,
            ScheduleService schedule,
            CancellationToken cancellationToken) =>
        {
            var response = await schedule.GetLessonHistoryAsync(id, cancellationToken);

            return response is null
                ? Results.NotFound(new { error = $"Lesson '{id}' not found." })
                : Results.Ok(response);
        });

        return endpoints;
    }
}
