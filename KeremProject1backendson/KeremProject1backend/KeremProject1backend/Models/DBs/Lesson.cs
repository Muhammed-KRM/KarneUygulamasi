using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.DBs;

public class Lesson
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } // "Matematik", "Fizik"

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Topic> Topics { get; set; }
}
