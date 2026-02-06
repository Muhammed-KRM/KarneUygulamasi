using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

/// <summary>
/// Kurum sahipliği ilişkisi - Many-to-Many
/// Bir kullanıcı birden fazla kurumun sahibi olabilir
/// Bir kurumun birden fazla sahibi olabilir
/// </summary>
public class InstitutionOwner
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int InstitutionId { get; set; }
    public Institution Institution { get; set; } = null!;

    /// <summary>
    /// İlk başvuruyu yapan ana sahip mi?
    /// </summary>
    public bool IsPrimaryOwner { get; set; } = false;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Bu sahibi kim ekledi (null ise kendi başvurdu)
    /// </summary>
    public int? AddedByUserId { get; set; }
}
