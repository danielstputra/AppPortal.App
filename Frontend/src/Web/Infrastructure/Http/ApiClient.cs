using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web.Infrastructure.Http;

/// <summary>
/// Real HTTP client implementation using IHttpClientFactory.
///
/// 🔐 Auth: Bearer, API Key, API Key + HMAC-SHA256, Basic
/// 🔄 Retry: Exponential backoff (200ms → 500ms → 1s) for 5xx errors
/// 📡 Proxy: Uses relative URLs so YARP forwards through frontend
/// 🆔 Tracing: Extracts traceId from "X-Trace-Id" response header
/// 📊 Response: Follows RFC 7807 (Problem Details) for errors
/// </summary>
public class ApiClient : IApiClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private AuthConfig? _authConfig;
    private readonly IWebHostEnvironment? _env;

    private const int MaxRetries = 3;
    private static readonly TimeSpan[] RetryDelays = { TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1) };

    public string BaseUrl { get; set; } = "/api";
    public AuthConfig? CurrentAuth => _authConfig;

    /// <summary>
    /// Auto-detected from IWebHostEnvironment. In Production mode,
    /// error responses are sanitized to prevent leaking internal details.
    /// </summary>
    public ApiEnvironmentMode EnvironmentMode { get; set; }

    public ApiClient(IHttpClientFactory httpClientFactory,
                     IHttpContextAccessor httpContextAccessor,
                     IWebHostEnvironment environment,
                     ILogger<ApiClient> logger)
    {
        var client = httpClientFactory.CreateClient("ApiClient");

        // Set BaseAddress to current origin so relative URLs work
        // through YARP proxy — backend URL stays hidden from client.
        var request = httpContextAccessor.HttpContext?.Request;
        if (request != null)
            client.BaseAddress = new Uri($"{request.Scheme}://{request.Host}");
        else
            client.BaseAddress = new Uri("http://localhost:5000");

        // Auto-detect environment mode
        _env = environment;
        EnvironmentMode = environment.IsDevelopment()
            ? ApiEnvironmentMode.Development
            : ApiEnvironmentMode.Production;

        _httpClient = client;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  AUTHENTICATION
    // ═══════════════════════════════════════════════════════════

    public void SetAuth(AuthConfig config)
    {
        _authConfig = config;
        _logger.LogDebug("Auth configured: {Method}", config.Method);
    }

    public void ClearAuth()
    {
        _authConfig = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
        _httpClient.DefaultRequestHeaders.Remove("X-API-Signature");
        _httpClient.DefaultRequestHeaders.Remove("X-API-Timestamp");
        _logger.LogDebug("Auth cleared");
    }

    /// <summary>
    /// Applies the current auth configuration to a request message.
    /// For HMAC-SHA256, the signature is computed per-request.
    /// </summary>
    private void ApplyAuth(HttpRequestMessage request, string? body)
    {
        if (_authConfig == null) return;

        switch (_authConfig.Method)
        {
            case AuthMethod.Bearer:
                if (!string.IsNullOrEmpty(_authConfig.BearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authConfig.BearerToken);
                }
                break;

            case AuthMethod.ApiKey:
                if (!string.IsNullOrEmpty(_authConfig.ApiKey))
                {
                    request.Headers.Add(_authConfig.ApiKeyHeaderName, _authConfig.ApiKey);
                }
                break;

            case AuthMethod.ApiKeyWithSecret:
                if (!string.IsNullOrEmpty(_authConfig.ApiKey) && !string.IsNullOrEmpty(_authConfig.ApiSecret))
                {
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var path = request.RequestUri?.PathAndQuery ?? "/";
                    var signature = _authConfig.ComputeSignature(request.Method.Method, path, body, timestamp);

                    request.Headers.Add(_authConfig.ApiKeyHeaderName, _authConfig.ApiKey);
                    request.Headers.Add(_authConfig.ApiSignatureHeaderName, signature);
                    request.Headers.Add(_authConfig.ApiTimestampHeaderName, timestamp.ToString());
                }
                break;

            case AuthMethod.Basic:
                if (!string.IsNullOrEmpty(_authConfig.Username) && !string.IsNullOrEmpty(_authConfig.Password))
                {
                    var credentials = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{_authConfig.Username}:{_authConfig.Password}")
                    );
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                }
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  HTTP METHODS
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        => await ExecuteAsync<T>(HttpMethod.Get, endpoint);

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null)
        => await ExecuteAsync<T>(HttpMethod.Post, endpoint, data);

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null)
        => await ExecuteAsync<T>(HttpMethod.Put, endpoint, data);

    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        => await ExecuteAsync<T>(HttpMethod.Delete, endpoint);

    public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null)
        => await ExecuteAsync<T>(HttpMethod.Patch, endpoint, data);

    // ═══════════════════════════════════════════════════════════
    //  CORE EXECUTION ENGINE
    // ═══════════════════════════════════════════════════════════

    private async Task<ApiResponse<T>> ExecuteAsync<T>(HttpMethod method, string endpoint, object? data = null)
    {
        var url = CombineUrl(BaseUrl, endpoint);
        var attempt = 0;

        while (attempt <= MaxRetries)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url);
                string? body = null;

                if (data != null)
                {
                    body = JsonSerializer.Serialize(data, _jsonOptions);
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }

                // Apply authentication (HMAC signing happens per-request)
                ApplyAuth(request, body);

                _logger.LogDebug("HTTP {Method} {Url} (attempt {Attempt})", method.Method, url, attempt + 1);

                using var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Extract traceId from response headers
                var traceId = response.Headers.TryGetValues("X-Trace-Id", out var traceValues)
                    ? traceValues.FirstOrDefault()
                    : null;

                var statusCode = (int)response.StatusCode;

                // ─── Success (2xx) ───
                if (response.IsSuccessStatusCode)
                {
                    // Try to deserialize as a wrapped response with "data" field first,
                    // then fall back to direct deserialization.
                    var wrapped = TryDeserializeWrapped<T>(content);
                    if (wrapped != null)
                    {
                        return ApiResponse<T>.Ok(wrapped.Data!, statusCode,
                            wrapped.Pagination, traceId ?? wrapped.TraceId);
                    }

                    // Direct deserialization
                    var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    if (result != null)
                        return ApiResponse<T>.Ok(result, statusCode, traceId: traceId);

                    // Empty body but success — return Ok with default
                    if (string.IsNullOrWhiteSpace(content))
                        return ApiResponse<T>.Ok(default(T)!, statusCode, traceId: traceId);

                    return ApiResponse<T>.ServerError("Failed to parse response", statusCode, traceId);
                }

                // ─── Error (4xx/5xx) — try to extract structured error ───
                var error = TryParseError(content, statusCode, traceId);
                if (error != null)
                {
                    // 🔒 Production: strip internal details from error before returning
                    if (EnvironmentMode == ApiEnvironmentMode.Production)
                        error = error.SanitizeForProduction();
                    return ApiResponse<T>.Fail(error, statusCode, traceId);
                }

                // ─── Known status codes ───
                if (statusCode == 401)
                    return ApiResponse<T>.Unauthorized(
                        EnvironmentMode == ApiEnvironmentMode.Development
                            ? "Unauthorized — token may be expired"
                            : "Authentication required", traceId);
                if (statusCode == 403)
                    return ApiResponse<T>.Forbidden(
                        EnvironmentMode == ApiEnvironmentMode.Development
                            ? "Forbidden — insufficient permissions"
                            : "Access denied", traceId);
                if (statusCode == 404)
                    return ApiResponse<T>.NotFound("Resource not found", traceId);
                if (statusCode == 429)
                    return ApiResponse<T>.RateLimited(
                        EnvironmentMode == ApiEnvironmentMode.Development
                            ? "Rate limit exceeded"
                            : "Too many requests. Try again later.", traceId);

                // ─── Server error (5xx) — retry ───
                if (statusCode >= 500 && attempt < MaxRetries)
                {
                    _logger.LogWarning("Server error {StatusCode} for {Url}, retrying ({Attempt}/{Max})",
                        statusCode, url, attempt + 1, MaxRetries);
                    await Task.Delay(RetryDelays[attempt]);
                    attempt++;
                    continue;
                }

                return ApiResponse<T>.ServerError($"HTTP {statusCode}: {response.ReasonPhrase}", statusCode, traceId);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Request timed out for {Url} (attempt {Attempt})", url, attempt + 1);
                if (attempt < MaxRetries)
                {
                    await Task.Delay(RetryDelays[attempt]);
                    attempt++;
                    continue;
                }
                return ApiResponse<T>.NetworkError("Request timed out after retries");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for {Url} (attempt {Attempt})", url, attempt + 1);
                if (attempt < MaxRetries)
                {
                    await Task.Delay(RetryDelays[attempt]);
                    attempt++;
                    continue;
                }
                return ApiResponse<T>.NetworkError($"Network error: {ex.Message}");
            }
        }

        return ApiResponse<T>.NetworkError("Max retries exceeded");
    }

    // ═══════════════════════════════════════════════════════════
    //  RESPONSE PARSING HELPERS
    // ═══════════════════════════════════════════════════════════

    // Model for wrapped API responses like { "data": ..., "pagination": ..., "traceId": ... }
    private class WrappedResponse<T>
    {
        public T? Data { get; set; }
        public PaginationMeta? Pagination { get; set; }
        public string? TraceId { get; set; }
    }

    // Model for RFC 7807 error responses
    private class ErrorResponse
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public string? TraceId { get; set; }
        public List<ErrorDetailItem>? Details { get; set; }
    }

    private class ErrorDetailItem
    {
        public string? Field { get; set; }
        public string? Message { get; set; }
        public string? Code { get; set; }
    }

    /// <summary>
    /// Try to deserialize a wrapped response with "data" envelope.
    /// Supports both: { "data": ... } and direct array/object.
    /// </summary>
    private WrappedResponse<T>? TryDeserializeWrapped<T>(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("data", out _))
            {
                return JsonSerializer.Deserialize<WrappedResponse<T>>(json, _jsonOptions);
            }
        }
        catch (JsonException) { }

        return null;
    }

    /// <summary>
    /// Try to extract a structured error from the response body.
    /// Supports RFC 7807 (Problem Details) and custom error formats.
    /// </summary>
    private static ApiErrorDetail? TryParseError(string json, int statusCode, string? traceId)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // RFC 7807: { "type", "title", "status", "detail", "errors" }
            // Google:  { "error": { "code": ..., "message": ..., "errors": [...] } }
            // Custom:  { "code": ..., "message": ... }

            string? code = null;
            string? message = null;
            List<FieldError>? fieldErrors = null;

            // Check for wrapped "error" envelope (Google-style)
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("error", out var errorEl))
            {
                code = errorEl.TryGetProperty("code", out var c) ? c.GetString() : null;
                message = errorEl.TryGetProperty("message", out var m) ? m.GetString() : null;

                if (errorEl.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Array)
                {
                    fieldErrors = ParseFieldErrors(errs);
                }
            }
            // RFC 7807 / flat format
            else
            {
                root.TryGetProperty("code", out var codeEl);
                root.TryGetProperty("title", out var titleEl);
                root.TryGetProperty("message", out var msgEl);
                root.TryGetProperty("detail", out var detailEl);

                code = codeEl.GetString()
                     ?? titleEl.GetString();

                message = msgEl.GetString()
                        ?? detailEl.GetString()
                        ?? titleEl.GetString();

                if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Array)
                {
                    fieldErrors = ParseFieldErrors(errs);
                }
            }

            if (!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(message))
            {
                return new ApiErrorDetail(
                    code ?? "ERROR",
                    message ?? $"HTTP {statusCode}",
                    fieldErrors
                );
            }
        }
        catch (JsonException) { }

        return null;
    }

    private static List<FieldError>? ParseFieldErrors(JsonElement errorsArray)
    {
        var errors = new List<FieldError>();
        foreach (var item in errorsArray.EnumerateArray())
        {
            var field = item.TryGetProperty("field", out var f) ? f.GetString() : null;
            var message = item.TryGetProperty("message", out var m) ? m.GetString() : null;
            var code = item.TryGetProperty("code", out var c) ? c.GetString() : "INVALID";
            errors.Add(new FieldError(field ?? "", message ?? "", code ?? "INVALID"));
        }
        return errors.Count > 0 ? errors : null;
    }

    private static string CombineUrl(string baseUrl, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return endpoint;
        baseUrl = baseUrl.TrimEnd('/');
        endpoint = endpoint.TrimStart('/');
        return $"{baseUrl}/{endpoint}";
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
