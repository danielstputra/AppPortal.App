using Web.Features.EmployeeManagement.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class EmployeeServicesRegistration
{
    public static IServiceCollection AddEmployeeServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        return services;
    }
}
