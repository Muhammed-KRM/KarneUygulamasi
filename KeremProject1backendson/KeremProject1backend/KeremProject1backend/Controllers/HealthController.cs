using KeremProject1backend.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationContext _context;
    private readonly IDistributedCache _cache;

    public HealthController(ApplicationContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            services = new
            {
                database = await CheckDatabaseAsync(),
                redis = await CheckRedisAsync()
            }
        };

        return Ok(health);
    }

    private async Task<string> CheckDatabaseAsync()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return "connected";
        }
        catch
        {
            return "disconnected";
        }
    }

    private async Task<string> CheckRedisAsync()
    {
        try
        {
            await _cache.SetStringAsync("health_check", "ok", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            });
            var value = await _cache.GetStringAsync("health_check");
            return value == "ok" ? "connected" : "disconnected";
        }
        catch
        {
            return "disconnected";
        }
    }
}

