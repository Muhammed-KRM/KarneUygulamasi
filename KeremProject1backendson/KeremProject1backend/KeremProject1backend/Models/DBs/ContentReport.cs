using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class ContentReport
{
    public int Id { get; set; }
    public int ContentId { get; set; }
    public Content Content { get; set; } = null!;

    public int UserId { get; set; } // Şikayet eden
    public User User { get; set; } = null!;

    public string Reason { get; set; } = string.Empty; // spam, inappropriate, harassment, copyright, other
    public string? Description { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    // İnceleme bilgileri
    public int? ReviewedBy { get; set; }
    public User? Reviewer { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

