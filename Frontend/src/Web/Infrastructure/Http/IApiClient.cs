namespace Web.Infrastructure.Http;

/// <summary>
/// Runtime environment mode — controls error detail visibility.
/// </summary>
public enum ApiEnvironmentMode
{
    /// <summary>Show full error details, server messages, field-level errors.</summary>
    Development,
    /// <summary>Sanitize errors: generic messages, no internal details exposed to client.</summary>
    Production
}

/// <summary>
/// Enterprise-grade HTTP client abstraction.
/// Supports Bearer, API Key, API Key + HMAC Secret, and Basic auth.
///
/// Swappable: use MockApiClient for development, ApiClient for production.
/// When the real backend is ready, swap the DI registration — zero code changes.
/// </summary>
public interface IApiClient
{
    // ─── Environment ───

    /// <summary>
    /// Current environment mode. In Production, error details are sanitized
    /// to prevent leaking internal information to the client.
    /// Auto-detected from IWebHostEnvironment if not explicitly set.
    /// </summary>
    ApiEnvironmentMode EnvironmentMode { get; set; }

    // ─── Authentication ───

    /// <summary>
    /// Configure authentication for subsequent requests.
    /// Supports: Bearer, API Key, API Key + HMAC Secret, Basic.
    /// </summary>
    void SetAuth(AuthConfig config);

    /// <summary>
    /// Clear all authentication headers.
    /// </summary>
    void ClearAuth();

    /// <summary>
    /// The current auth configuration, if set.
    /// </summary>
    AuthConfig? CurrentAuth { get; }

    // ─── HTTP Methods ───

    /// <summary>GET request. Returns enterprise-standard ApiResponse.</summary>
    Task<ApiResponse<T>> GetAsync<T>(string endpoint);

    /// <summary>POST request with JSON body.</summary>
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null);

    /// <summary>PUT request with JSON body.</summary>
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null);

    /// <summary>DELETE request.</summary>
    Task<ApiResponse<T>> DeleteAsync<T>(string endpoint);

    /// <summary>PATCH request with JSON body.</summary>
    Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null);

    // ─── Configuration ───

    /// <summary>
    /// Base URL for all requests (e.g. "/api/v1").
    /// Uses relative path so YARP proxy forwards through the frontend.
    /// </summary>
    string BaseUrl { get; set; }
}
