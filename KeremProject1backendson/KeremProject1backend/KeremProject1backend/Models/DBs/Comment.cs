namespace KeremProject1backend.Models.DBs;

public class Comment
{
    public int Id { get; set; }
    public int ContentId { get; set; }
    public Content Content { get; set; } = null!;

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    // Nested Comments (Yanıtlar)
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    // Denormalized Counts
    public int LikesCount { get; set; } = 0;
    public int RepliesCount { get; set; } = 0;

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

