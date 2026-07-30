using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DashboardServicesRegistration
{
    public static IServiceCollection AddDashboardServices(this IServiceCollection services)
    {
        // Currently no specific dashboard services
        return services;
    }
}
