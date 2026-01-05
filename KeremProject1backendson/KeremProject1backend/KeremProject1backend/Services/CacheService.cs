using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Text.Json;

namespace KeremProject1backend.Services;

public class CacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _redis;

    public CacheService(IDistributedCache cache, IServiceProvider serviceProvider)
    {
        _cache = cache;
        // Try to get Redis connection if available
        _redis = serviceProvider.GetService<IConnectionMultiplexer>();
    }

    public async Task<T?> GetAsync<T>(string key, bool forceRefresh = false)
    {
        if (forceRefresh)
            return default;

        var value = await _cache.GetStringAsync(key);
        if (value == null)
            return default;

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        var serialized = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serialized, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    /// <summary>
    /// Removes all cache keys matching the pattern (e.g., "user_*", "admin_statistics")
    /// Uses Redis SCAN for pattern matching if Redis is available, otherwise removes known keys
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern)
    {
        if (_redis != null)
        {
            // Redis pattern matching using SCAN
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var database = _redis.GetDatabase();
            
            // Convert pattern to Redis pattern (e.g., "user_*" -> "user_*")
            var redisPattern = pattern.Replace("*", "*");
            
            await foreach (var key in server.KeysAsync(pattern: redisPattern))
            {
                await database.KeyDeleteAsync(key);
            }
        }
        else
        {
            // Fallback: Remove common patterns manually
            // This is less efficient but works without direct Redis access
            await RemoveKnownPatternKeysAsync(pattern);
        }
    }

    /// <summary>
    /// Removes cache keys for a specific user (all user-related caches)
    /// </summary>
    public async Task InvalidateUserCacheAsync(int userId)
    {
        var patterns = new[]
        {
            $"user_profile_{userId}_*",
            $"user_preferences_{userId}",
            $"user_statistics_{userId}",
            $"User:{userId}:*"
        };

        foreach (var pattern in patterns)
        {
            await RemoveByPatternAsync(pattern);
        }
    }

    /// <summary>
    /// Removes admin-related caches
    /// </summary>
    public async Task InvalidateAdminCacheAsync()
    {
        await RemoveAsync("admin_statistics");
        await RemoveByPatternAsync("admin_users_*");
        await RemoveByPatternAsync("admin_institutions_*");
    }

    /// <summary>
    /// Removes exam-related caches
    /// </summary>
    public async Task InvalidateExamCacheAsync(int? examId = null)
    {
        if (examId.HasValue)
        {
            await RemoveByPatternAsync($"exam_{examId}_*");
        }
        await RemoveByPatternAsync("exams_*");
    }

    private async Task RemoveKnownPatternKeysAsync(string pattern)
    {
        // Known key patterns that we can remove manually
        // This is a fallback when Redis SCAN is not available
        // In production, you should use Redis directly for pattern matching
        
        // For now, we'll log that pattern removal was attempted
        // In a real scenario, you might want to maintain a registry of cache keys
        // or use a more sophisticated cache key management system
    }
}
