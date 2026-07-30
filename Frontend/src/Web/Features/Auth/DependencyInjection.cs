using Microsoft.AspNetCore.Components.Authorization;
using Web.Features.Auth.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuthServicesRegistration
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddCascadingAuthenticationState();
        services.AddAuthorizationCore();

        services.AddScoped<AppAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AppAuthenticationStateProvider>());
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}

