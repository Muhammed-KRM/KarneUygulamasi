using Hangfire;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Jobs;

/// <summary>
/// Background job to clean up old read notifications
/// Scheduled to run weekly to keep the database clean
/// </summary>
public class NotificationCleanupJob
{
    private readonly ApplicationContext _context;

    public NotificationCleanupJob(ApplicationContext context)
    {
        _context = context;
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task Execute()
    {
        // Remove notifications older than 30 days that are already read
        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        var oldNotifications = await _context.Notifications
            .Where(n => n.IsRead && n.CreatedAt < cutoffDate)
            .ToListAsync();

        if (oldNotifications.Any())
        {
            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();
        }
    }
}
