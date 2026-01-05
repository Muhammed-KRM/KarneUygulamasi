namespace KeremProject1backend.Models.DBs;

public class StoryReaction
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public Story Story { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Reaction { get; set; } = string.Empty; // "👍", "❤️", "😊", vb.

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

