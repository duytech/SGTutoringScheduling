using Microsoft.EntityFrameworkCore;
using SynergieGlobalTutoringScheduling.Data;
using SynergieGlobalTutoringScheduling.Endpoints.Bookings;
using SynergieGlobalTutoringScheduling.Endpoints.Conflicts;
using SynergieGlobalTutoringScheduling.Endpoints.Schedule;
using SynergieGlobalTutoringScheduling.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<BookingValidationService>();
builder.Services.AddScoped<ScheduleService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, app.Environment.ContentRootPath);
}

app.MapGet("/", () => "Hello World!");
app.MapValidateBookingEndpoint();
app.MapConflictsEndpoint();
app.MapScheduleEndpoint();

app.Run();
