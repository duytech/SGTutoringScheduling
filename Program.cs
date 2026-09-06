using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Data;
using TutoringScheduling.Endpoints.Conflicts;
using TutoringScheduling.Endpoints.Schedule;
using TutoringScheduling.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<ScheduleService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, app.Environment.ContentRootPath);
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapConflictsEndpoint();
app.MapScheduleEndpoint();

app.Run();
