namespace KeremProject1backend.Models.DTOs.Responses;

public class StoryDto
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorImageUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? Text { get; set; }
    public int ViewsCount { get; set; }
    public int ReactionsCount { get; set; }
    public bool IsViewed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

