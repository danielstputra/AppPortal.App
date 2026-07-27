using System.Text.Json;

namespace Web.Services.Http;

/// <summary>
/// Mock HTTP client — reads from local wwwroot/mock-data/*.json files.
/// Simulates API responses with realistic latency, trace IDs, and structured data.
///
/// When the real backend is ready, swap this for ApiClient in DI — zero code changes.
/// </summary>
public class MockApiClient : IApiClient
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MockApiClient> _logger;
    private AuthConfig? _authConfig;
    private readonly Random _rng = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public string BaseUrl { get; set; } = "/mock-data";
    public AuthConfig? CurrentAuth => _authConfig;

    /// <summary>
    /// Auto-detected from IWebHostEnvironment. In Production mode,
    /// error responses are sanitized.
    /// </summary>
    public ApiEnvironmentMode EnvironmentMode { get; set; }

    public MockApiClient(IWebHostEnvironment env, ILogger<MockApiClient> logger)
    {
        _env = env;
        _logger = logger;
        EnvironmentMode = env.IsDevelopment()
            ? ApiEnvironmentMode.Development
            : ApiEnvironmentMode.Production;
    }

    public void SetAuth(AuthConfig config)
    {
        _authConfig = config;
        _logger.LogDebug("Mock: Auth configured — {Method}", config.Method);
    }

    public void ClearAuth()
    {
        _authConfig = null;
        _logger.LogDebug("Mock: Auth cleared");
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        => await MockDelayThen(() => LoadFromJson<T>(endpoint));

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null)
        => await MockDelayThen(() =>
        {
            _logger.LogDebug("Mock POST {Endpoint}", endpoint);
            return ApiResponse<T>.Created(default!, GenerateTraceId());
        });

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null)
        => await MockDelayThen(() =>
        {
            _logger.LogDebug("Mock PUT {Endpoint}", endpoint);
            return ApiResponse<T>.Ok(default!, traceId: GenerateTraceId());
        });

    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        => await MockDelayThen(() =>
        {
            _logger.LogDebug("Mock DELETE {Endpoint}", endpoint);
            return ApiResponse<T>.Ok(default!, traceId: GenerateTraceId());
        });

    public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null)
        => await MockDelayThen(() =>
        {
            _logger.LogDebug("Mock PATCH {Endpoint}", endpoint);
            return ApiResponse<T>.Ok(default!, traceId: GenerateTraceId());
        });

    // ─── Private Helpers ───

    private async Task<ApiResponse<T>> MockDelayThen<T>(Func<ApiResponse<T>> action)
    {
        // Simulate realistic network latency (100-400ms)
        var delay = _rng.Next(100, 400);
        await Task.Delay(delay);
        return action();
    }

    private ApiResponse<T> LoadFromJson<T>(string endpoint)
    {
        // Convert endpoint path to file path: "/employees" → "employees.json"
        var fileName = endpoint.TrimStart('/').TrimEnd('/');
        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            fileName += ".json";

        var filePath = Path.Combine(_env.WebRootPath, "mock-data", fileName);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Mock data file not found: {Path}", filePath);
            var msg = EnvironmentMode == ApiEnvironmentMode.Development
                ? $"Mock data file '{fileName}' not found"
                : "The requested resource was not found.";
            return ApiResponse<T>.NotFound(msg, GenerateTraceId());
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var traceId = GenerateTraceId();

            // Try wrapped response format first: { "data": [...], "pagination": {...} }
            var wrapped = TryDeserializeWrapped<T>(json);
            if (wrapped != null)
            {
                var pagination = wrapped.Pagination ?? new PaginationMeta(1, wrapped.Data is System.Collections.ICollection c ? c.Count : 0, 0);
                _logger.LogDebug("Mock: Loaded {Type} from {File} (wrapped)", typeof(T).Name, fileName);
                return ApiResponse<T>.Ok(wrapped.Data!, 200, pagination, traceId);
            }

            // Direct deserialization
            var data = JsonSerializer.Deserialize<T>(json, JsonOptions);
            if (data != null)
            {
                _logger.LogDebug("Mock: Loaded {Type} from {File} (direct)", typeof(T).Name, fileName);
                return ApiResponse<T>.Ok(data, 200, traceId: traceId);
            }

            var deserMsg = EnvironmentMode == ApiEnvironmentMode.Development
                ? $"Failed to deserialize {fileName} into {typeof(T).Name}"
                : "An internal server error occurred. Please try again.";
            return ApiResponse<T>.ServerError(deserMsg, traceId: traceId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parse error in {File}", fileName);
            var jsonMsg = EnvironmentMode == ApiEnvironmentMode.Development
                ? $"Invalid JSON in '{fileName}': {ex.Message}"
                : "An internal server error occurred. Please try again.";
            return ApiResponse<T>.ServerError(jsonMsg, traceId: GenerateTraceId());
        }
    }

    private class WrappedResponse<T>
    {
        public T? Data { get; set; }
        public PaginationMeta? Pagination { get; set; }
    }

    private static WrappedResponse<T>? TryDeserializeWrapped<T>(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("data", out _))
            {
                return JsonSerializer.Deserialize<WrappedResponse<T>>(json, JsonOptions);
            }
        }
        catch (JsonException) { }
        return null;
    }

    private string GenerateTraceId()
    {
        var bytes = new byte[16];
        _rng.NextBytes(bytes);
        return $"mock-{Convert.ToHexString(bytes).ToLowerInvariant()}";
    }
}
