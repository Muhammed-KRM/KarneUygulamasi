using System.ComponentModel.DataAnnotations;
using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class Message
{
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public required Conversation Conversation { get; set; } = null!;

    public int SenderId { get; set; }
    public required User Sender { get; set; } = null!;

    [Required]
    public required string Content { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;

    public int? AttachedExamId { get; set; } // Share an exam
    public Exam? AttachedExam { get; set; }

    public int? AttachedExamResultId { get; set; } // Share a report card
    public ExamResult? AttachedExamResult { get; set; }

    public string? AttachmentUrl { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
