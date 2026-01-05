using System.Diagnostics;
using System.Text;

namespace KeremProject1backend.Middlewares;

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
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        
        // Get User ID from claims if authenticated
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

        // Log request start
        _logger.LogInformation(
            "Request started: {Method} {Path} | IP: {IpAddress} | UserId: {UserId}",
            requestMethod, requestPath, ipAddress, userId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Log request completion
            _logger.LogInformation(
                "Request completed: {Method} {Path} | Status: {StatusCode} | Duration: {ElapsedMs}ms | IP: {IpAddress} | UserId: {UserId}",
                requestMethod, requestPath, statusCode, elapsedMs, ipAddress, userId);

            // Log slow requests (over 1 second)
            if (elapsedMs > 1000)
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} | Duration: {ElapsedMs}ms | IP: {IpAddress} | UserId: {UserId}",
                    requestMethod, requestPath, elapsedMs, ipAddress, userId);
            }
        }
    }
}

