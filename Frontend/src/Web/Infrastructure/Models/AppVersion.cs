namespace Web.Infrastructure.Models;

/// <summary>
/// Application version information — single source of truth.
/// Update this file when releasing a new version.
/// </summary>
public static class AppVersion
{
    /// <summary>Major.Minor.Patch — follows Semantic Versioning 2.0.</summary>
    public const string Version = "1.0.0";

    /// <summary>Build metadata (optional — auto-increment or CI pipeline).</summary>
    public const string Build = "001";

    /// <summary>Environment label shown in UI.</summary>
    public static string DisplayName => $"v{Version}";

    /// <summary>Full version string with build number.</summary>
    public static string Full => $"{Version}+build.{Build}";

    /// <summary>Copyright notice.</summary>
    public const string Copyright = "© 2026 Portal Aplikasi. All rights reserved.";
}
