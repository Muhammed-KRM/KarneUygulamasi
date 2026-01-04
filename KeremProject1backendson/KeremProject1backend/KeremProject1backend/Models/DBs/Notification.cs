using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public required User User { get; set; }

    public required string Title { get; set; }
    public required string Message { get; set; }

    public string? ActionUrl { get; set; } // Action URL

    public NotificationType Type { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
