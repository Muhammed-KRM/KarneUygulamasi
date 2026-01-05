using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class Content
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public ContentType ContentType { get; set; } // Question, Post, Announcement
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? FileUrl { get; set; }

    // Ders ve Konu
    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public int? TopicId { get; set; }
    public Topic? Topic { get; set; }

    // Zorluk seviyesi (sorular için)
    public DifficultyLevel? Difficulty { get; set; }

    // Hashtag'ler (JSON array: ["matematik", "tyt", "soru"])
    public string? TagsJson { get; set; }

    // Denormalized Counts (Performans için)
    public int ViewsCount { get; set; } = 0;
    public int LikesCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;
    public int SavesCount { get; set; } = 0;

    // Çözüldü mü? (sorular için)
    public bool IsSolved { get; set; } = false;
    public int? SolvedByUserId { get; set; }
    public User? SolvedByUser { get; set; }
    public DateTime? SolvedAt { get; set; }

    // Pinning (Content Sabitleme)
    public bool IsPinned { get; set; } = false;
    public DateTime? PinnedAt { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
}

