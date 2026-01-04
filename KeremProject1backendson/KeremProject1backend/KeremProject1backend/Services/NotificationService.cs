using KeremProject1backend.Hubs;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Enums;
using Microsoft.AspNetCore.SignalR;

namespace KeremProject1backend.Services;

public class NotificationService
{
    private readonly ApplicationContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(ApplicationContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(int userId, string title, string message, NotificationType type, string? link = null)
    {
        // Persist to DB
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ActionUrl = link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            User = null!
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Send via SignalR
        await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
    }
}
