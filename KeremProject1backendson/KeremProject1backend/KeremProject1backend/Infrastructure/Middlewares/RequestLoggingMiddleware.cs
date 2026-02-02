using System.Diagnostics;
using System.Text;

namespace KeremProject1backend.Infrastructure.Middlewares;

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
        var request = context.Request;

        _logger.LogInformation("HTTP Request: {Method} {Path}", request.Method, request.Path);

        await _next(context);

        stopwatch.Stop();
        var response = context.Response;

        _logger.LogInformation("HTTP Response: {StatusCode} in {ElapsedMilliseconds}ms", 
            response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}
