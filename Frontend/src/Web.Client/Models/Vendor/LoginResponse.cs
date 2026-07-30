namespace Web.Models.Vendor;

public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn, UserProfile User);
public record TokenRefreshResponse(string AccessToken, string? RefreshToken, int ExpiresIn);
public record UserProfile(string Id, string Username, string Email, string DisplayName, string[] Roles);
