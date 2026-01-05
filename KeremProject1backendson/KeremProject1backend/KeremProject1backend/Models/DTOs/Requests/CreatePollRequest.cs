namespace KeremProject1backend.Models.DTOs.Requests;

public class CreatePollRequest
{
    public int ContentId { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new(); // En az 2, en fazla 10 seçenek
    public DateTime? ExpiresAt { get; set; } // Null ise 7 gün sonra expire
    public bool IsMultipleChoice { get; set; } = false;
    public bool IsAnonymous { get; set; } = false;
}

