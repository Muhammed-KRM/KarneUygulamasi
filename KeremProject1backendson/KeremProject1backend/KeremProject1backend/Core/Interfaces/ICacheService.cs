namespace KeremProject1backend.Core.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null);
        Task RemoveAsync(string key);
    }
}
