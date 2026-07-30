using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Web.Infrastructure.Security;

public interface ITokenService
{
    Task SetTokensAsync(string accessToken, string refreshToken);
    Task ClearTokensAsync();
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
}

public class TokenService : ITokenService
{
    private readonly ProtectedLocalStorage _storage;
    private string? _memoryToken;
    private string? _memoryRefreshToken;

    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";

    public TokenService(ProtectedLocalStorage storage)
    {
        _storage = storage;
    }

    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        _memoryToken = accessToken;
        _memoryRefreshToken = refreshToken;
        try
        {
            await _storage.SetAsync(AccessTokenKey, accessToken);
            await _storage.SetAsync(RefreshTokenKey, refreshToken);
        }
        catch { /* in-memory fallback */ }
    }

    public async Task ClearTokensAsync()
    {
        _memoryToken = null;
        _memoryRefreshToken = null;
        try
        {
            await _storage.DeleteAsync(AccessTokenKey);
            await _storage.DeleteAsync(RefreshTokenKey);
        }
        catch { }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_memoryToken)) return _memoryToken;
        try
        {
            var result = await _storage.GetAsync<string>(AccessTokenKey);
            if (result.Success && result.Value != null)
            {
                _memoryToken = result.Value;
                return _memoryToken;
            }
        }
        catch { }
        return null;
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        if (!string.IsNullOrEmpty(_memoryRefreshToken)) return _memoryRefreshToken;
        try
        {
            var result = await _storage.GetAsync<string>(RefreshTokenKey);
            if (result.Success && result.Value != null)
            {
                _memoryRefreshToken = result.Value;
                return _memoryRefreshToken;
            }
        }
        catch { }
        return null;
    }
}
