using SynergieGlobalTutoringScheduling.Contracts;
using SynergieGlobalTutoringScheduling.Services;

namespace SynergieGlobalTutoringScheduling.Endpoints.Bookings;

public static class ValidateBookingEndpoint
{
    public static IEndpointRouteBuilder MapValidateBookingEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/bookings/validate", async (
            ValidateBookingRequest request,
            BookingValidationService validationService,
            CancellationToken cancellationToken) =>
        {
            var response = await validationService.ValidateAsync(request, cancellationToken);
            return Results.Ok(response);
        });

        return endpoints;
    }
}
