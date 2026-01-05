using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DTOs.Responses;

public class ContentDto
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorImageUrl { get; set; }
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
    public int ViewsCount { get; set; }
    public int LikesCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public int SavesCount { get; set; }
    public bool IsSolved { get; set; }
    public bool IsLiked { get; set; }
    public bool IsSaved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

