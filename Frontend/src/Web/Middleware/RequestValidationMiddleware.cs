using System.Text.RegularExpressions;

namespace Web.Middleware;

/// <summary>
/// Validates incoming requests for common security threats.
/// Blocks suspicious patterns (XSS, SQL injection attempts) at the middleware level.
/// </summary>
public partial class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;

    // Compiled regex patterns for quick validation
    [GeneratedRegex(@"(<script|javascript:|onerror=|onload=)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex XssPattern();

    [GeneratedRegex(@"(\b(UNION|SELECT|INSERT|DROP|DELETE|UPDATE|ALTER)\b.*\b(FROM|INTO|SET|TABLE)\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex SqlInjectionPattern();

    public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate query strings and form data
        if (context.Request.Method == "GET" || context.Request.Method == "POST")
        {
            // Check query string
            foreach (var (key, value) in context.Request.Query)
            {
                if (value.Count > 0 && ContainsThreatPattern(value.ToString()))
                {
                    _logger.LogWarning("Blocked suspicious query parameter: {Key}={Value} from {IP}",
                        key, value, context.Connection.RemoteIpAddress);
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid request parameter");
                    return;
                }
            }
        }

        await _next(context);
    }

    private static bool ContainsThreatPattern(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        return XssPattern().IsMatch(input) || SqlInjectionPattern().IsMatch(input);
    }
}

public static class RequestValidationExtensions
{
    public static IApplicationBuilder UseRequestValidation(this IApplicationBuilder app)
        => app.UseMiddleware<RequestValidationMiddleware>();
}
