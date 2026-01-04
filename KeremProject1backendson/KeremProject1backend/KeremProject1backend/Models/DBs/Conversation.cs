using System.ComponentModel.DataAnnotations;
using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class Conversation
{
    public int Id { get; set; }

    public ConversationType Type { get; set; } = ConversationType.Private;

    [MaxLength(200)]
    public string? Title { get; set; } // Renamed from Name in previous stepor null for private

    public bool IsGroup { get; set; } = false;

    public int? InstitutionId { get; set; } // Optional: Group can be linked to an institution
    public Institution? Institution { get; set; }

    public int? ClassroomId { get; set; } // Optional: Auto-group for classroom
    public Classroom? Classroom { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
