using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Infrastructure.Persistence;
using TutoringScheduling.Infrastructure.Time;

namespace TutoringScheduling.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the SQLite database, the pinned clock and the EF Core-backed
    /// implementation of <see cref="IScheduleStore"/>.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddSingleton<IClock, PinnedClock>();
        services.AddScoped<IScheduleStore, ScheduleStore>();

        return services;
    }
}
