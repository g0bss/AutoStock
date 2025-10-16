using System.Diagnostics;
using System.Security.Claims;

namespace DealershipInventorySystem.WebAPI.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid().ToString();

        // Add correlation ID to response headers
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // Log request
        var request = context.Request;
        var user = context.User.Identity?.Name ?? "Anonymous";
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

        _logger.LogInformation(
            "HTTP {Method} {Path} started by user {User} (ID: {UserId}) - Correlation ID: {CorrelationId}",
            request.Method,
            request.Path,
            user,
            userId,
            correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log response
            _logger.LogInformation(
                "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms - Correlation ID: {CorrelationId}",
                request.Method,
                request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);

            // Log slow requests
            if (stopwatch.ElapsedMilliseconds > 5000) // 5 seconds
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {ElapsedMilliseconds}ms - Correlation ID: {CorrelationId}",
                    request.Method,
                    request.Path,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);
            }
        }
    }
}