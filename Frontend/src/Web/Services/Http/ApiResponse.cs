using System.Text.Json;

namespace Web.Services.Http;

/// <summary>
/// Enterprise-standard API response wrapper.
/// Follows RFC 7807 (Problem Details) for errors, Google-style pagination,
/// and includes traceId + timestamp for distributed debugging.
///
/// Usage:
///   var response = await api.GetAsync<Employee>("/employees");
///   if (response.IsSuccess) { var data = response.Data; }
///   else { var error = response.Error; }
/// </summary>
public class ApiResponse<T>
{
    /// <summary>Whether the API call succeeded (HTTP 2xx).</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Deserialized response payload. Null if the request failed.</summary>
    public T? Data { get; init; }

    /// <summary>Standardized error details (RFC 7807). Null if the request succeeded.</summary>
    public ApiErrorDetail? Error { get; init; }

    /// <summary>Pagination metadata for list endpoints. Null for single items.</summary>
    public PaginationMeta? Pagination { get; init; }

    /// <summary>
    /// Unique identifier for distributed tracing.
    /// Maps to "X-Trace-Id" response header or "traceId" in error JSON.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>ISO 8601 timestamp of when the response was received.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>The raw HTTP status code from the response.</summary>
    public int HttpStatusCode { get; init; }

    // ─── Convenience Accessors ───

    public bool IsError => !IsSuccess;
    public bool HasData => IsSuccess && Data is not null;
    public bool HasPagination => Pagination is not null;

    // ─── Factory Methods ───

    /// <summary>Successful response (HTTP 2xx).</summary>
    public static ApiResponse<T> Ok(T data, int statusCode = 200, PaginationMeta? pagination = null, string? traceId = null) => new()
    {
        IsSuccess = true,
        Data = data,
        Pagination = pagination,
        TraceId = traceId,
        HttpStatusCode = statusCode
    };

    /// <summary>Created response (HTTP 201).</summary>
    public static ApiResponse<T> Created(T data, string? traceId = null) => new()
    {
        IsSuccess = true,
        Data = data,
        HttpStatusCode = 201,
        TraceId = traceId
    };

    /// <summary>Error response with RFC 7807 Problem Details.</summary>
    public static ApiResponse<T> Fail(ApiErrorDetail error, int statusCode, string? traceId = null) => new()
    {
        IsSuccess = false,
        Error = error,
        HttpStatusCode = statusCode,
        TraceId = traceId
    };

    /// <summary>Not Found (HTTP 404).</summary>
    public static ApiResponse<T> NotFound(string message = "Resource not found", string? traceId = null)
        => Fail(new ApiErrorDetail("NOT_FOUND", message), 404, traceId);

    /// <summary>Bad Request (HTTP 400) with optional field-level validation errors.</summary>
    public static ApiResponse<T> BadRequest(string message, List<FieldError>? fieldErrors = null, string? traceId = null)
        => Fail(new ApiErrorDetail("BAD_REQUEST", message, fieldErrors), 400, traceId);

    /// <summary>Unauthorized (HTTP 401).</summary>
    public static ApiResponse<T> Unauthorized(string message = "Unauthorized", string? traceId = null)
        => Fail(new ApiErrorDetail("UNAUTHORIZED", message), 401, traceId);

    /// <summary>Forbidden (HTTP 403).</summary>
    public static ApiResponse<T> Forbidden(string message = "Forbidden", string? traceId = null)
        => Fail(new ApiErrorDetail("FORBIDDEN", message), 403, traceId);

    /// <summary>Rate Limited (HTTP 429).</summary>
    public static ApiResponse<T> RateLimited(string message = "Rate limit exceeded", string? traceId = null)
        => Fail(new ApiErrorDetail("RATE_LIMITED", message), 429, traceId);

    /// <summary>Internal Server Error (HTTP 5xx).</summary>
    public static ApiResponse<T> ServerError(string message = "Internal server error", int statusCode = 500, string? traceId = null)
        => Fail(new ApiErrorDetail("SERVER_ERROR", message), statusCode, traceId);

    /// <summary>Network / timeout error (no HTTP response received).</summary>
    public static ApiResponse<T> NetworkError(string message, string? traceId = null)
        => Fail(new ApiErrorDetail("NETWORK_ERROR", message), 503, traceId);
}

/// <summary>
/// RFC 7807 Problem Details — enterprise-standard error envelope.
/// </summary>
public class ApiErrorDetail
{
    /// <summary>Machine-readable error code (e.g. "VALIDATION_ERROR", "NOT_FOUND").</summary>
    public string Code { get; init; }

    /// <summary>Human-readable error description.</summary>
    public string Message { get; init; }

    /// <summary>Field-level validation errors (for 400 Bad Request).</summary>
    public List<FieldError>? Details { get; init; }

    /// <summary>Optional URI pointing to error documentation.</summary>
    public string? HelpUrl { get; init; }

    public ApiErrorDetail(string code, string message, List<FieldError>? details = null, string? helpUrl = null)
    {
        Code = code;
        Message = message;
        Details = details;
        HelpUrl = helpUrl;
    }

    /// <summary>
    /// Returns a sanitized copy suitable for production — strips internal server details,
    /// field-level validation errors, and help URLs that might leak internal information.
    /// </summary>
    public ApiErrorDetail SanitizeForProduction() => new(
        Code,
        GetSafeMessage(),
        details: null,       // Field errors may contain internal schema details
        helpUrl: null        // Internal help URLs may leak architecture info
    );

    /// <summary>
    /// Maps error codes to safe, generic messages that reveal no internal details.
    /// </summary>
    private string GetSafeMessage() => Code.ToUpperInvariant() switch
    {
        "NOT_FOUND"         => "The requested resource was not found.",
        "UNAUTHORIZED"      => "Authentication is required to access this resource.",
        "FORBIDDEN"         => "You do not have permission to perform this action.",
        "VALIDATION_ERROR"  => "The request contains invalid fields.",
        "RATE_LIMITED"      => "Too many requests. Please try again later.",
        "SERVER_ERROR"      => "An internal server error occurred. Please try again.",
        "NETWORK_ERROR"     => "A network error occurred. Please check your connection.",
        "TIMEOUT"           => "The request timed out. Please try again.",
        _                   => "An unexpected error occurred. Please try again."
    };
}

/// <summary>
/// Field-level validation error (for form validation).
/// </summary>
public class FieldError
{
    /// <summary>The field name that failed validation.</summary>
    public string Field { get; init; }

    /// <summary>Human-readable validation message.</summary>
    public string Message { get; init; }

    /// <summary>Machine-readable error code (e.g. "REQUIRED", "INVALID_FORMAT").</summary>
    public string Code { get; init; }

    public FieldError(string field, string message, string code = "INVALID")
    {
        Field = field;
        Message = message;
        Code = code;
    }
}

/// <summary>
/// Pagination metadata for list endpoints. Follows Google-style pagination.
/// </summary>
public class PaginationMeta
{
    /// <summary>Current page number (1-based).</summary>
    public int Page { get; init; }

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>Total number of items across all pages.</summary>
    public int Total { get; init; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages { get; init; }

    /// <summary>Whether there is a next page.</summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>Whether there is a previous page.</summary>
    public bool HasPrevious => Page > 1;

    public PaginationMeta(int page, int pageSize, int total)
    {
        Page = Math.Max(1, page);
        PageSize = Math.Max(1, pageSize);
        Total = Math.Max(0, total);
        TotalPages = Total > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
    }
}
