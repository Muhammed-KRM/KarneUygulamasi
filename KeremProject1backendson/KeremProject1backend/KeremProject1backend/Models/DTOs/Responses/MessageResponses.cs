using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DTOs.Responses;

public class MessageDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public int? AttachedExamId { get; set; }
    public int? AttachedExamResultId { get; set; }
    public DateTime SentAt { get; set; }
}
