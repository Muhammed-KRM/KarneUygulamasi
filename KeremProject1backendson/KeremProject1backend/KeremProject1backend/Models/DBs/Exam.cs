using System.ComponentModel.DataAnnotations;
using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class Exam
{
    public int Id { get; set; }

    public int InstitutionId { get; set; }
    public required Institution Institution { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; } // TYT Deneme-1

    public DateTime ExamDate { get; set; }
    public ExamType Type { get; set; }

    public int? ClassroomId { get; set; } // Optional: Can be school-wide
    public Classroom? Classroom { get; set; }

    // Answer Key (JSON): { "Matematik": "ABCDE...", "Fizik": "BCD..." }
    public required string AnswerKeyJson { get; set; }

    // Lesson Config (JSON): { "Matematik": { "StartIndex": 0, "QuestionCount": 40, "TopicMapping": { "0-9": "Fonksiyonlar" } } }
    public required string LessonConfigJson { get; set; }

    public bool IsPublished { get; set; } = false;

    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
}
