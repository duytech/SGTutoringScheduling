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
            // gets its own shape rather than growing the shared ApiError. Status is
            // still 200 — the client tells this apart from success via error.code.
            if (result.Error is MoveConflictError conflictError)
            {
                return Results.Ok(new
                {
                    data = (MoveLessonResponse?)null,
                    error = new ApiConflictError(conflictError.Code, conflictError.Message, conflictError.Conflicts),
                });
            }

            return result.ToHttpResult();
        });

        return endpoints;
    }
}
