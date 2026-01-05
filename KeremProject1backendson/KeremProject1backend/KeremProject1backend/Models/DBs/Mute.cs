namespace KeremProject1backend.Models.DBs;

public class Mute
{
    public int Id { get; set; }
    public int UserId { get; set; } // Sessizleştiren
    public User User { get; set; } = null!;

    public int MutedUserId { get; set; } // Sessizleştirilen
    public User MutedUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

