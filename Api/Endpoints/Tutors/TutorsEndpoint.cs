using TutoringScheduling.Application;

namespace TutoringScheduling.Api.Endpoints.Tutors;

public static class TutorsEndpoint
{
    public static IEndpointRouteBuilder MapTutorsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var tutorRoutes = endpoints.MapGroup("/api/tutors");

        tutorRoutes.MapGet("/loads", async (
            DateOnly date,
            TutorService tutors,
            CancellationToken cancellationToken) =>
        {
            var response = await tutors.GetLoadsForDayAsync(date, cancellationToken);
            return Results.Ok(response);
        });

        return endpoints;
    }
}
