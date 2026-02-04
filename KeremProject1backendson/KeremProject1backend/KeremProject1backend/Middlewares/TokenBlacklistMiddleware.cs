using KeremProject1backend.Services;
using System.IdentityModel.Tokens.Jwt;

namespace KeremProject1backend.Middlewares;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public TokenBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CacheService cacheService)
    {
        // Only check for authenticated requests
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                // Check if token is blacklisted
                var isBlacklisted = await cacheService.GetAsync<bool>($"Blacklist:Token:{token}");

                if (isBlacklisted)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        error = "Token has been revoked",
                        errorCode = "100000"
                    });
                    return;
                }
            }
        }

        await _next(context);
    }
}

