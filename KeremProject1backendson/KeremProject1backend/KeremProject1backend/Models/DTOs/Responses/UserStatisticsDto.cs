namespace KeremProject1backend.Models.DTOs.Responses;

public class UserStatisticsDto
{
    public int TotalExams { get; set; }
    public float AverageScore { get; set; }
    public float AverageNet { get; set; }
    public string? BestLesson { get; set; }
    public string? MostImprovedTopic { get; set; }
    public int TotalMessages { get; set; }
    public int TotalNotifications { get; set; }
    public DateTime? LastExamDate { get; set; }
    public int? BestRank { get; set; }
}

