using Microsoft.Extensions.DependencyInjection;

namespace TutoringScheduling.Application;

public static class DependencyInjection
{
    /// <summary>Registers the use-case services. Persistence comes from Infrastructure.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<ITutorService, TutorService>();
        services.AddScoped<IMoveLessonService, MoveLessonService>();

        return services;
    }
}
