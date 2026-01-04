using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class InstitutionUser
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int InstitutionId { get; set; }
    public Institution Institution { get; set; } = null!;

    public InstitutionRole Role { get; set; }

    // Öğrenci için
    public string? StudentNumber { get; set; } // Kurum öğrenci numarası

    // Öğretmen için
    public string? EmployeeNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
