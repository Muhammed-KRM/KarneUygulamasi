using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;

namespace KeremProject1backend.Services;

public class AuditService
{
    private readonly ApplicationContext _context;

    public AuditService(ApplicationContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int? userId, string action, string? details = null, string? ipAddress = null, string? userAgent = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
