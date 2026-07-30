using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Web.Infrastructure;

public class AppConfiguration
{
    private readonly IConfiguration _configuration;
    private readonly IWebAssemblyHostEnvironment _env;

    public AppConfiguration(IConfiguration configuration, IWebAssemblyHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    public string VendorApiUrl =>
        _configuration["ApiSettings:VendorApiBaseUrl"]
        ?? throw new InvalidOperationException("Missing VendorApiBaseUrl");

    public bool IsDevelopment => _env.IsDevelopment();
    public bool IsProduction => _env.IsProduction();
}
