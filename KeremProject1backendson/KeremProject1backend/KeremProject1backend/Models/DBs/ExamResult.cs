namespace KeremProject1backend.Models.DBs;

public class ExamResult
{
    public int Id { get; set; }

    public int ExamId { get; set; }
    public required Exam Exam { get; set; } = null!;

    public int StudentId { get; set; } // Points to User ID
    public required User Student { get; set; } = null!;

    public required string StudentNumber { get; set; } // For optical match
    public required string BookletType { get; set; } // "A", "B" etc.

    // Detailed Results (JSON): { "Matematik": { "Correct": 30, "Wrong": 5, "Net": 28.75 } }
    public required string DetailedResultsJson { get; set; }

    public int TotalCorrect { get; set; }
    public int TotalWrong { get; set; }
    public int TotalEmpty { get; set; }
    public float TotalNet { get; set; }
    public float TotalScore { get; set; }

    public int? ClassRank { get; set; }
    public int? InstitutionRank { get; set; }

    public bool IsConfirmed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
}
