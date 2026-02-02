using KeremProject1backend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.DTOs.Requests;

public class StartConversationRequest
{
    public int InstitutionId { get; set; }
    public int? ClassroomId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
}

public class SendMessageRequest
{
    public int ConversationId { get; set; }
    [Required]
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public int? AttachedExamId { get; set; }
    public int? AttachedResultId { get; set; }
}

public class SendToClassRequest
{
    public int ClassroomId { get; set; }
    public List<int> ReportCardIds { get; set; } = new();
}
