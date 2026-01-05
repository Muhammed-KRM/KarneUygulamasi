namespace KeremProject1backend.Models.DTOs.Responses;

public class UserActivityDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // Login, Exam, Message, etc.
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? RelatedEntityId { get; set; } // ExamId, MessageId, etc.
    public string? RelatedEntityType { get; set; } // Exam, Message, etc.
}

