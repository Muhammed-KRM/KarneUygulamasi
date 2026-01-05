namespace KeremProject1backend.Models.DBs;

public class Poll
{
    public int Id { get; set; }
    public int ContentId { get; set; }
    public Content Content { get; set; } = null!;

    public string Question { get; set; } = string.Empty;
    public string OptionsJson { get; set; } = string.Empty; // ["Seçenek 1", "Seçenek 2", ...]
    public DateTime ExpiresAt { get; set; }
    public bool IsMultipleChoice { get; set; } = false;
    public bool IsAnonymous { get; set; } = false;
    public int TotalVotes { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
}

