using KeremProject1backend.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace KeremProject1backend.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var stringValue = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(stringValue))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(stringValue);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions();

            options.AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(60);
            options.SlidingExpiration = unusedExpireTime;

            var stringValue = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, stringValue, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
