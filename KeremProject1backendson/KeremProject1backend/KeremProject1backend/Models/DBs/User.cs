using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public UserRole GlobalRole { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Active;

    public string? ProfileImageUrl { get; set; }
    public ProfileVisibility ProfileVisibility { get; set; } = ProfileVisibility.PublicToAll;

    // Denormalized Counts (Performans için)
    public int FollowerCount { get; set; } = 0;
    public int FollowingCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties
    public ICollection<InstitutionUser> InstitutionMemberships { get; set; } = new List<InstitutionUser>();
    public ICollection<AccountLink> AccountLinks { get; set; } = new List<AccountLink>();
}
