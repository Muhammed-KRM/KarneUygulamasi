using KeremProject1backend.Services;
using System.IdentityModel.Tokens.Jwt;

namespace KeremProject1backend.Middlewares;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CacheService _cacheService;

    public TokenBlacklistMiddleware(RequestDelegate next, CacheService cacheService)
    {
        _next = next;
        _cacheService = cacheService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only check for authenticated requests
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (!string.IsNullOrEmpty(token))
            {
                // Check if token is blacklisted
                var isBlacklisted = await _cacheService.GetAsync<bool>($"Blacklist:Token:{token}");
                
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

