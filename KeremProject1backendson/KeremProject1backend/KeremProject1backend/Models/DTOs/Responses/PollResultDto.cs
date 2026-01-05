namespace KeremProject1backend.Models.DTOs.Responses;

public class PollResultDto
{
    public int PollId { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<PollOptionResultDto> Options { get; set; } = new();
    public int TotalVotes { get; set; }
    public bool IsExpired { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class PollOptionResultDto
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public double Percentage { get; set; } // 0-100 arası
}

