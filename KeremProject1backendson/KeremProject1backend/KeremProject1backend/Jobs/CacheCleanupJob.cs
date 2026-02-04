using Hangfire;
using KeremProject1backend.Services;

namespace KeremProject1backend.Jobs;

/// <summary>
/// Background job to clean up expired cache entries
/// Scheduled to run daily to keep Redis memory usage optimal
/// </summary>
public class CacheCleanupJob
{
    private readonly CacheService _cacheService;

    public CacheCleanupJob(CacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task Execute()
    {
        // Remove old exam caches (older than 24 hours)
        await _cacheService.RemoveByPatternAsync("exam_results_*");
        await _cacheService.RemoveByPatternAsync("exam_detail_*");

        // Remove old classroom caches (older than 24 hours)
        await _cacheService.RemoveByPatternAsync("Classroom:*:Students:*");

        // Note: User permissions and classroom details have their own TTL
        // and don't need manual cleanup
    }
}
