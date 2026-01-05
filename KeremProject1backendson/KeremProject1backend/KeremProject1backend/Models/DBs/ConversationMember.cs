namespace KeremProject1backend.Models.DBs;

public class ConversationMember
{
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public required Conversation Conversation { get; set; } = null!;

    public int UserId { get; set; }
    public required User User { get; set; } = null!;

    public bool IsAdmin { get; set; } = false;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReadAt { get; set; }
}
