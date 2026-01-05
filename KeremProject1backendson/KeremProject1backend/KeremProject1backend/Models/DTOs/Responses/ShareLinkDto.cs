namespace KeremProject1backend.Models.DTOs.Responses;

public class ShareLinkDto
{
    public string ShareLink { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

