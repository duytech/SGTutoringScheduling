using SynergieGlobalTutoringScheduling.Contracts;

namespace SynergieGlobalTutoringScheduling.Endpoints.Bookings;

public static class ValidateBookingEndpoint
{
    public static IEndpointRouteBuilder MapValidateBookingEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/bookings/validate", (ValidateBookingRequest request) =>
        {
            var response = new ValidateBookingResponse
            {
                Valid = true
            };

            return Results.Ok(response);
        });

        return endpoints;
    }
}
