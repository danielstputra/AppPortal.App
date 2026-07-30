using Web.Features.Auth.Models;

namespace Web.Features.Auth.Services;

public interface IAuthService
{
    Task<bool> IsAuthenticatedAsync();
    Task<LoginResponse?> LoginAsync(string username, string password);
    Task<bool> RefreshTokenAsync();
    Task Logout();
    bool IsAuthenticated { get; }
}
