using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Web.Models.Vendor;

namespace Web.Infrastructure.Auth;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _js;
    private readonly NavigationManager _navigation;
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "auth_refresh_token";

    public UserProfile? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;

    public AuthService(IHttpClientFactory httpClientFactory, IJSRuntime js, NavigationManager navigation)
    {
        _httpClient = httpClientFactory.CreateClient("VendorApi");
        _js = js;
        _navigation = navigation;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", new { username, password });
            if (!response.IsSuccessStatusCode) return false;
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result is null) return false;
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, result.AccessToken);
            await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, result.RefreshToken);
            CurrentUser = result.User;
            return true;
        }
        catch (HttpRequestException) { return false; }
    }

    public async Task TryRestoreSessionAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
        if (string.IsNullOrEmpty(token)) return;
        try
        {
            var response = await _httpClient.GetAsync("auth/me");
            if (response.IsSuccessStatusCode)
                CurrentUser = await response.Content.ReadFromJsonAsync<UserProfile>();
        }
        catch { await LogoutAsync(); }
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
        _navigation.NavigateTo("/login", forceLoad: true);
    }

    public async Task<string?> GetAccessTokenAsync() =>
        await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);

    public async Task<string?> GetRefreshTokenAsync() =>
        await _js.InvokeAsync<string>("localStorage.getItem", RefreshTokenKey);

    public async Task<string?> RefreshTokenAsync()
    {
        var refreshToken = await GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken)) return null;
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/refresh", new { refreshToken });
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<TokenRefreshResponse>();
            if (result is null) return null;
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, result.AccessToken);
            if (result.RefreshToken is not null)
                await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, result.RefreshToken);
            return result.AccessToken;
        }
        catch { await LogoutAsync(); return null; }
    }
}
