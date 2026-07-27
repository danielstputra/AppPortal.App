using System.Security.Cryptography;
using System.Text;

namespace Web.Services.Http;

/// <summary>
/// Supported authentication methods for enterprise API communication.
/// </summary>
public enum AuthMethod
{
    /// <summary>No authentication (public endpoints).</summary>
    None,
    /// <summary>Bearer JWT token: "Authorization: Bearer {token}"</summary>
    Bearer,
    /// <summary>API Key via custom header: "X-API-Key: {key}"</summary>
    ApiKey,
    /// <summary>API Key + HMAC-SHA256 signature for request integrity.</summary>
    ApiKeyWithSecret,
    /// <summary>Basic Auth: "Authorization: Basic {base64(username:password)}"</summary>
    Basic
}

/// <summary>
/// Enterprise authentication configuration for IApiClient.
/// Supports Bearer, API Key, API Key + HMAC Secret, and Basic auth.
/// </summary>
public class AuthConfig
{
    // ─── Auth Method ───
    public AuthMethod Method { get; set; } = AuthMethod.Bearer;

    // ─── Bearer Token ───
    public string? BearerToken { get; set; }

    // ─── API Key + Secret ───
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }

    /// <summary>
    /// Custom header name for API Key (default: "X-API-Key").
    /// </summary>
    public string ApiKeyHeaderName { get; set; } = "X-API-Key";

    /// <summary>
    /// Custom header name for API Secret / signature (default: "X-API-Signature").
    /// </summary>
    public string ApiSignatureHeaderName { get; set; } = "X-API-Signature";

    /// <summary>
    /// Custom header name for timestamp used in HMAC signing (default: "X-API-Timestamp").
    /// </summary>
    public string ApiTimestampHeaderName { get; set; } = "X-API-Timestamp";

    // ─── Basic Auth ───
    public string? Username { get; set; }
    public string? Password { get; set; }

    // ─── Factory Methods ───

    public static AuthConfig FromBearer(string token) => new()
    {
        Method = AuthMethod.Bearer,
        BearerToken = token
    };

    public static AuthConfig FromApiKey(string apiKey) => new()
    {
        Method = AuthMethod.ApiKey,
        ApiKey = apiKey
    };

    /// <summary>
    /// API Key + HMAC-SHA256 signing. The signature is computed from:
    /// timestamp + method + path + body and signed with apiSecret.
    /// </summary>
    public static AuthConfig FromApiKeyWithSecret(string apiKey, string apiSecret) => new()
    {
        Method = AuthMethod.ApiKeyWithSecret,
        ApiKey = apiKey,
        ApiSecret = apiSecret
    };

    public static AuthConfig FromBasic(string username, string password) => new()
    {
        Method = AuthMethod.Basic,
        Username = username,
        Password = password
    };

    // ─── HMAC Signature Helpers ───

    /// <summary>
    /// Computes HMAC-SHA256 signature for request integrity verification.
    /// Format: timestamp + httpMethod + path + bodyHash
    /// </summary>
    public string ComputeSignature(string httpMethod, string path, string? body, long timestamp)
    {
        if (string.IsNullOrEmpty(ApiSecret))
            throw new InvalidOperationException("ApiSecret is required for HMAC signing");

        var bodyHash = string.IsNullOrEmpty(body)
            ? string.Empty
            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();

        var payload = $"{timestamp}{httpMethod.ToUpperInvariant()}{path}{bodyHash}";

        var keyBytes = Encoding.UTF8.GetBytes(ApiSecret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        var hmac = new HMACSHA256(keyBytes);
        var signatureBytes = hmac.ComputeHash(payloadBytes);
        hmac.Dispose();

        return Convert.ToHexString(signatureBytes).ToLowerInvariant();
    }
}
