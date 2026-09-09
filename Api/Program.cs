using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Api.Endpoints.Conflicts;
using TutoringScheduling.Api.Endpoints.Lessons;
using TutoringScheduling.Api.Endpoints.Schedule;
using TutoringScheduling.Api.Endpoints.Tutors;
using TutoringScheduling.Application;
using TutoringScheduling.Infrastructure;
using TutoringScheduling.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, AppContext.BaseDirectory);
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapConflictsEndpoint();
app.MapScheduleEndpoint();
app.MapTutorsEndpoint();
app.MapMoveLessonEndpoint();
app.MapLessonHistoryEndpoint();

app.Run();
