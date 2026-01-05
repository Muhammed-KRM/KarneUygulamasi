namespace KeremProject1backend.Models.DTOs.Responses;

public class CommentDto
{
    public int Id { get; set; }
    public int ContentId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorImageUrl { get; set; }
    public string Text { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
    public int LikesCount { get; set; }
    public int RepliesCount { get; set; }
    public bool IsLiked { get; set; }
    public List<CommentDto>? Replies { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

