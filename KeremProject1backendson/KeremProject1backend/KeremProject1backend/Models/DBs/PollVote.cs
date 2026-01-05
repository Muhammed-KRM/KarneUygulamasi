namespace KeremProject1backend.Models.DBs;

public class PollVote
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public Poll Poll { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int OptionIndex { get; set; } // 0, 1, 2, ...
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

