using System.Text.Json.Serialization;

namespace Web.Features.Auth.Models;

public class LoginResponse
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("tokenType")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expiresIn")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refreshTokenExpiresIn")]
    public int RefreshTokenExpiresIn { get; set; }
}
