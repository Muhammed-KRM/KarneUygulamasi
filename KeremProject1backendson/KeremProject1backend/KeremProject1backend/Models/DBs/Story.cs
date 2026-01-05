namespace KeremProject1backend.Models.DBs;

public class Story
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string? ImageUrl { get; set; } // Görsel story
    public string? VideoUrl { get; set; } // Video story
    public string? Text { get; set; } // Metin story (max 200 karakter)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } // CreatedAt + 24 saat

    // Denormalized Counts
    public int ViewsCount { get; set; } = 0;
    public int ReactionsCount { get; set; } = 0;

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    public ICollection<StoryView> Views { get; set; } = new List<StoryView>();
    public ICollection<StoryReaction> Reactions { get; set; } = new List<StoryReaction>();
}

