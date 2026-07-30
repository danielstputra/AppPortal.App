using System.Net.Http.Headers;

namespace Web.Infrastructure.Auth;

public class AuthDelegatingHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public AuthDelegatingHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath?.Contains("/auth/login") == true ||
            request.RequestUri?.AbsolutePath?.Contains("/auth/refresh") == true)
            return await base.SendAsync(request, cancellationToken);

        var token = await _authService.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var newToken = await _authService.RefreshTokenAsync();
            if (newToken is not null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response = await base.SendAsync(request, cancellationToken);
            }
        }
        return response;
    }
}
