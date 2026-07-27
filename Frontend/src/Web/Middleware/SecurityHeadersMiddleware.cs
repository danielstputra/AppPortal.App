using System.Text.RegularExpressions;

namespace Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) { _next = next; }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' https://cdn.tailwindcss.com https://cdn.jsdelivr.net 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' https://cdn.jsdelivr.net https://fonts.googleapis.com https://cdn.tailwindcss.com 'unsafe-inline'; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "img-src 'self' data: blob:; " +
            "connect-src 'self' ws: wss: https://cdn.jsdelivr.net https://cdn.tailwindcss.com https://fonts.googleapis.com https://fonts.gstatic.com; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'";

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["X-XSS-Protection"] = "1; mode=block";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), fullscreen=(self), payment=(), usb=()";
        headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
        headers["Pragma"] = "no-cache";

        await _next(context);
    }
}

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
