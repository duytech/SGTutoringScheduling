using TutoringScheduling.Application;
using TutoringScheduling.Application.Contracts;

namespace TutoringScheduling.Api.Endpoints.Lessons;

public static class MoveLessonEndpoint
{
    public static IEndpointRouteBuilder MapMoveLessonEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/lessons/{id}/move", async (
            string id,
            MoveLessonRequest request,
            MoveLessonService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.MoveAsync(id, request, cancellationToken);

            return result.Outcome switch
            {
                MoveOutcome.Applied => Results.Ok(result.Applied),
                MoveOutcome.LessonNotFound => Results.NotFound(new { error = $"Lesson '{id}' not found." }),
                MoveOutcome.Rejected => Results.Json(
                    new MoveRejectedResponse
                    {
                        Reason = result.RejectionReason!,
                        Conflicts = result.Conflicts.ToList(),
                    },
                    statusCode: StatusCodes.Status409Conflict),
                _ => Results.Problem("Unexpected move outcome."),
            };
        });

        return endpoints;
    }
}
