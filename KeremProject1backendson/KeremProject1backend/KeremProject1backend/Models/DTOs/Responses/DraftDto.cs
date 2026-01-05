using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DTOs.Responses;

public class DraftDto
{
    public int Id { get; set; }
    public ContentType ContentType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? FileUrl { get; set; }
    public int? LessonId { get; set; }
    public string? LessonName { get; set; }
    public int? TopicId { get; set; }
    public string? TopicName { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime LastSavedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

