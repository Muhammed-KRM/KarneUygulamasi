namespace KeremProject1backend.Models.DTOs.Requests;

public class CreateCommentRequest
{
    public string Text { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; } // Nested comment için
}

