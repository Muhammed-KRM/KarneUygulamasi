namespace KeremProject1backend.Models.DBs;

public class Follow
{
    public int Id { get; set; }
    public int FollowerId { get; set; } // Takip eden
    public User Follower { get; set; } = null!;

    public int FollowingId { get; set; } // Takip edilen
    public User Following { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

