using SynergieGlobalTutoringScheduling.Endpoints.Bookings;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapValidateBookingEndpoint();

app.Run();
