using TutoringScheduling.Api.Contracts;
using TutoringScheduling.Api.Extensions;
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
            IMoveLessonService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.MoveAsync(id, request, cancellationToken);

            // Every other outcome fits the shared envelope; a blocking clash carries
            // its conflicts too (mirrors the old MoveRejectedResponse contract), so it
            // gets its own shape rather than growing the shared ApiError.
            if (result.Error is MoveConflictError conflictError)
            {
                return Results.Json(
                    new
                    {
                        data = (MoveLessonResponse?)null,
                        error = new ApiConflictError(conflictError.Code, conflictError.Message, conflictError.Conflicts),
                    },
                    statusCode: StatusCodes.Status409Conflict);
            }

            return result.ToHttpResult();
        });

        return endpoints;
    }
}
