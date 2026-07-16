using AppPortal.App.Services;

namespace AppPortal.App.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<LoadingService>();
        services.AddScoped<DialogService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<ThemeService>();
        services.AddScoped<NavigationService>();
        services.AddScoped<BreadcrumbService>();
        services.AddScoped<UserSessionService>();

        return services;
    }
}
