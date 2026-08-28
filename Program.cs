using SynergieGlobalTutoringScheduling.Endpoints.Bookings;
using SynergieGlobalTutoringScheduling.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<JsonScheduleStore>();
builder.Services.AddScoped<BookingValidationService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapValidateBookingEndpoint();

app.Run();
