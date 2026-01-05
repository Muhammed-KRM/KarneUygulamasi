namespace KeremProject1backend.Models.DBs;

public class EmailVerification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

