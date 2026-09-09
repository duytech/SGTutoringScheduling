using Microsoft.Extensions.DependencyInjection;

namespace TutoringScheduling.Application;

public static class DependencyInjection
{
    /// <summary>Registers the use-case services. Persistence comes from Infrastructure.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ScheduleService>();
        services.AddScoped<TutorService>();
        services.AddScoped<MoveLessonService>();

        return services;
    }
}
