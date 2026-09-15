namespace api_gestion_productos.Middleware;

using Serilog.Context;

/// <summary>
/// X-Request-Id entrante se reutiliza; si no viene, se genera.
/// Queda en el response y en el logger scope para correlacionar.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Request-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var id = context.Request.Headers.TryGetValue(HeaderName, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.ToString()
            : context.TraceIdentifier;

        context.Items[HeaderName] = id;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = id;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope("{RequestId}", id))
        using (LogContext.PushProperty("RequestId", id))
        {
            await _next(context);
        }
    }
}

/// <summary>Headers mínimos de seguridad para una API.</summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var h = context.Response.Headers;
            h["X-Content-Type-Options"] = "nosniff";
            h["X-Frame-Options"] = "DENY";
            h["Referrer-Policy"] = "no-referrer";
            h["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
            return Task.CompletedTask;
        });
        await _next(context);
    }
}

public static class HttpHardeningExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder b)
        => b.UseMiddleware<CorrelationIdMiddleware>();
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder b)
        => b.UseMiddleware<SecurityHeadersMiddleware>();
}
