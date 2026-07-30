using Microsoft.Extensions.DependencyInjection;
using Web.Infrastructure.Http;
using Web.Infrastructure.Localization;
using Web.Infrastructure.Security;

namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServicesRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IApiClient, ApiClient>();
        services.AddScoped<LocalizationService>();
        services.AddScoped<ITokenService, TokenService>();
        return services;
    }
}
