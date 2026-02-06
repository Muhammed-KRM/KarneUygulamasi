using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class Institution
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty; // MEB Ruhsat No
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public InstitutionStatus Status { get; set; } = InstitutionStatus.PendingApproval;
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByAdminId { get; set; }

    // Navigation Properties
    public ICollection<InstitutionUser> Members { get; set; } = new List<InstitutionUser>();
    public ICollection<InstitutionOwner> Owners { get; set; } = new List<InstitutionOwner>();
}

