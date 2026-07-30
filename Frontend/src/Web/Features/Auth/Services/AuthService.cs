using Microsoft.AspNetCore.Components.Authorization;
using Web.Infrastructure.Security;
using Web.Infrastructure.Http;
using Web.Features.Auth.Models;

namespace Web.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;
    private readonly ITokenService _tokenService;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<AuthService> _logger;
    private string? _token;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public AuthService(
        IApiClient apiClient, 
        ITokenService tokenService, 
        AuthenticationStateProvider authStateProvider,
        ILogger<AuthService> logger)
    {
        _apiClient = apiClient; 
        _tokenService = tokenService; 
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        if (!string.IsNullOrEmpty(_token)) return true;
        var stored = await _tokenService.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(stored))
        {
            _token = stored;
            _apiClient.SetAuth(new AuthConfig { Method = AuthMethod.Bearer, BearerToken = _token });
            return true;
        }
        return false;
    }

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest { Username = username, Password = password };
            var response = await _apiClient.PostAsync<LoginResponse>("/Users/Login", request);
            if (!response.IsSuccess || response.Data == null)
            { 
                _logger.LogWarning("Login failed"); 
                return null; 
            }

            _token = response.Data.AccessToken;
            await _tokenService.SetTokensAsync(
                response.Data.AccessToken ?? "",
                response.Data.RefreshToken ?? ""
            );

            if (!string.IsNullOrEmpty(_token))
            {
                _apiClient.SetAuth(new AuthConfig { Method = AuthMethod.Bearer, BearerToken = _token });
                if (_authStateProvider is AppAuthenticationStateProvider customProvider)
                {
                    customProvider.NotifyUserAuthentication(_token);
                }
            }

            return response.Data;
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "Login failed"); 
            return null; 
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var refreshRequest = new { RefreshToken = refreshToken };
            var response = await _apiClient.PostAsync<LoginResponse>("/Users/RefreshToken", refreshRequest);

            if (!response.IsSuccess || response.Data?.AccessToken == null)
            { 
                _logger.LogWarning("Token refresh failed"); 
                return false; 
            }

            _token = response.Data.AccessToken;
            await _tokenService.SetTokensAsync(response.Data.AccessToken, response.Data.RefreshToken ?? "");
            _apiClient.SetAuth(new AuthConfig { Method = AuthMethod.Bearer, BearerToken = _token });

            if (_authStateProvider is AppAuthenticationStateProvider customProvider)
            {
                customProvider.NotifyUserAuthentication(_token);
            }

            return true;
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "Token refresh failed"); 
            return false; 
        }
    }

    public async Task Logout()
    {
        try
        {
            var storedRefresh = await _tokenService.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(storedRefresh))
            {
                try { await _apiClient.PostAsync<object>("/Users/Logout"); }
                catch { /* ignore API errors */ }
            }
        }
        catch { }

        _token = null;
        await _tokenService.ClearTokensAsync();
        _apiClient.ClearAuth();

        if (_authStateProvider is AppAuthenticationStateProvider customProvider)
        {
            customProvider.NotifyUserLogout();
        }
    }
}
