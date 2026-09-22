using TutoringScheduling.Api.Extensions;
using TutoringScheduling.Application;

namespace TutoringScheduling.Api.Endpoints.Tutors;

public static class TutorsEndpoint
{
    public static IEndpointRouteBuilder MapTutorsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var tutorRoutes = endpoints.MapGroup("/api/tutors");

        tutorRoutes.MapGet("/loads", async (
            DateOnly date,
            ITutorService tutors,
            CancellationToken cancellationToken) =>
        {
            var result = await tutors.GetLoadsForDayAsync(date, cancellationToken);
            return result.ToHttpResult();
        });

        return endpoints;
    }
}
