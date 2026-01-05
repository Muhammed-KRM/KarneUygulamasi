using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class ContentDraft
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public ContentType ContentType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? FileUrl { get; set; }
    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public int? TopicId { get; set; }
    public Topic? Topic { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
    public string? TagsJson { get; set; }

    public DateTime LastSavedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

