using System.Diagnostics;

namespace KeremProject1backend.Middlewares
{
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

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var response = context.Response;

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {Elapsed:0.0000} ms. User: {User}",
                    request.Method,
                    request.Path,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    context.User?.Identity?.Name ?? "Anonymous"
                );
            }
        }
    }
}
