namespace KeremProject1backend.Models.DTOs.Responses;

public class PollDto
{
    public int Id { get; set; }
    public int ContentId { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
    public bool IsMultipleChoice { get; set; }
    public bool IsAnonymous { get; set; }
    public int TotalVotes { get; set; }
    public bool HasVoted { get; set; } // Kullanıcı oy vermiş mi?
    public List<int>? UserVotes { get; set; } // Kullanıcının seçtiği seçenekler (eğer anonymous değilse)
    public DateTime CreatedAt { get; set; }
}

