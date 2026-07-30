using System.Net.Http.Json;
using System.Text.Json;
using Web.Models.Vendor;

namespace Web.Infrastructure.Http;

public class VendorHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VendorHttpClient> _logger;

    public VendorHttpClient(HttpClient httpClient, ILogger<VendorHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<T> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint, ct);
            return await HandleResponseAsync<T>(response, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GET {Endpoint} failed — network error", endpoint);
            throw new VendorApiException("Network error", inner: ex);
        }
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint, TRequest body, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, body, ct);
            return await HandleResponseAsync<TResponse>(response, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "POST {Endpoint} failed — network error", endpoint);
            throw new VendorApiException("Network error", inner: ex);
        }
    }

    private async Task<T> HandleResponseAsync<T>(
        HttpResponseMessage response, CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await TryReadErrorBodyAsync(response, ct);
            throw new VendorApiException(
                $"API Error: {(int)response.StatusCode} {response.ReasonPhrase}",
                statusCode: (int)response.StatusCode,
                errorBody: errorBody);
        }

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        if (envelope is null || !envelope.Success)
        {
            throw new VendorApiException(
                envelope?.Message ?? "API returned unsuccessful response",
                errorBody: envelope?.Errors is not null ? string.Join("; ", envelope.Errors) : null);
        }

        return envelope.Data!;
    }

    private static async Task<string?> TryReadErrorBodyAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        try { return await response.Content.ReadAsStringAsync(ct); }
        catch { return null; }
    }
}

public class VendorApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorBody { get; }

    public VendorApiException(string message, int statusCode = 0, string? errorBody = null, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ErrorBody = errorBody;
    }
}
