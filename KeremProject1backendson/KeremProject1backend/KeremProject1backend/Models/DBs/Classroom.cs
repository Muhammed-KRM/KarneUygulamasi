using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.DBs;

public class Classroom
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; } // e.g., "12-A", "LGS-Haftaiçi"

    public int Grade { get; set; } // 9, 10, 11, 12, or 8 (LGS)

    public int InstitutionId { get; set; }
    public required Institution Institution { get; set; }

    public int? HeadTeacherId { get; set; }
    public InstitutionUser? HeadTeacher { get; set; }

    public Conversation? ClassConversation { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ClassroomStudent> Students { get; set; } = new List<ClassroomStudent>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
