using System.Globalization;
using SynergieGlobalTutoringScheduling.Services;

namespace SynergieGlobalTutoringScheduling.Endpoints.Schedule;

public static class ScheduleEndpoint
{
    // The brief pins "today" to a date inside the seeded week rather than the
    // real clock; the fallback here matches the value documented in the README.
    private const string DefaultToday = "2026-03-06";

    public static IEndpointRouteBuilder MapScheduleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/schedule", async (
            DateOnly? date,
            ScheduleService schedule,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            var target = date ?? DateOnly.ParseExact(
                configuration["Schedule:Today"] ?? DefaultToday,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

            var response = await schedule.GetDayAsync(target, cancellationToken);
            return Results.Ok(response);
        });

        return endpoints;
    }
}
