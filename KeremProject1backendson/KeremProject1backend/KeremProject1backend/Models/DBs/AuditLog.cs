namespace KeremProject1backend.Models.DBs;

public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; } // Nullable for system actions
    public string Action { get; set; } = string.Empty; // "UserRegistered", "ExamCreated", etc.
    public string? Details { get; set; } // JSON or additional info
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
