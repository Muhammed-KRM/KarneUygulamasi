using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class AccountLink
{
    public int Id { get; set; }

    public int MainUserId { get; set; } // Bağımsız ana hesap
    public User MainUser { get; set; } = null!;

    public int InstitutionUserId { get; set; } // Kurum hesabı
    public InstitutionUser InstitutionUser { get; set; } = null!;

    public LinkStatus Status { get; set; } = LinkStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedByUserId { get; set; } // Onaylayan kurum yöneticisi
}
