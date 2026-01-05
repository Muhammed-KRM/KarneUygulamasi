namespace KeremProject1backend.Models.DTOs.Responses;

public class HashtagDetailDto
{
    public string Tag { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public int ContentCount { get; set; }
    public bool Trending { get; set; }
}

