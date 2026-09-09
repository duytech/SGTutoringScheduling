using TutoringScheduling.Application;

namespace TutoringScheduling.Api.Endpoints.Tutors;

public static class TutorsEndpoint
{
    public static IEndpointRouteBuilder MapTutorsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var tutors = endpoints.MapGroup("/api/tutors");

        tutors.MapGet("/loads", async (
            DateOnly date,
            TutorLoadService tutorLoads,
            CancellationToken cancellationToken) =>
        {
            var response = await tutorLoads.GetForDayAsync(date, cancellationToken);
            return Results.Ok(response);
        });

        return endpoints;
    }
}
