namespace KeremProject1backend.Models.DTOs.Responses;

public class StoryGroupDto
{
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorImageUrl { get; set; }
    public List<StoryDto> Stories { get; set; } = new List<StoryDto>();
}

