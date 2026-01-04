using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.DBs;

public class Topic
{
    public int Id { get; set; }

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } // "Fonksiyonlar", "Türev"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
