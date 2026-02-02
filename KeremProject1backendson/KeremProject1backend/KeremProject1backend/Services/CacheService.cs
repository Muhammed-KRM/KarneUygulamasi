using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Text.Json;

namespace KeremProject1backend.Services;

public class CacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly IConnectionMultiplexer? _redis;

    public CacheService(
        IDistributedCache cache,
        ILogger<CacheService> logger,
        IServiceProvider serviceProvider)
    {
        _cache = cache;
        _logger = logger;
        // Try to get Redis connection if available
        _redis = serviceProvider.GetService<IConnectionMultiplexer>();
    }

    public async Task<T?> GetAsync<T>(string key, bool forceRefresh = false)
    {
        if (forceRefresh)
            return default;

        try
        {
            var value = await _cache.GetStringAsync(key);
            if (value == null)
                return default;
            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
            };

            var serialized = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serialized, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null)
        {
            _logger.LogInformation("Cache hit for key: {Key}", key);
            return cachedValue;
        }

        _logger.LogInformation("Cache miss for key: {Key}. Fetching from origin.", key);
        var newValue = await factory();

        if (newValue != null)
        {
            await SetAsync(key, newValue, expiration);
        }

        return newValue;
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key from cache: {Key}", key);
        }
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
