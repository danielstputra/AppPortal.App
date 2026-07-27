using System.Text.Json;

namespace Web.Middleware;

/// <summary>
/// Global exception handler — catches unhandled exceptions, logs them,
/// and returns a sanitized JSON response (production) or detailed error (development).
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught by GlobalExceptionMiddleware");

            if (context.Response.HasStarted)
            {
                // Can't modify response once headers are sent — just log
                return;
            }

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            object errorObj;
            if (_env.IsDevelopment())
            {
                errorObj = new { code = "SERVER_ERROR", message = ex.Message, detail = ex.ToString() };
            }
            else
            {
                errorObj = new { code = "SERVER_ERROR", message = "An internal server error occurred. Please try again." };
            }

            var json = JsonSerializer.Serialize(new { error = errorObj }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}

public static class GlobalExceptionExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}
