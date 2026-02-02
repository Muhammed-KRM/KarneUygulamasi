namespace KeremProject1backend.Models.DTOs.Responses;

using KeremProject1backend.Models.Enums;

public class CreateExamDto
{
    public int InstitutionId { get; set; }
    public int? ClassroomId { get; set; }
    public required string Title { get; set; }
    public ExamType Type { get; set; }
    public DateTime ExamDate { get; set; }
    public string AnswerKeyJson { get; set; } = "{}";
    public string LessonConfigJson { get; set; } = "{}";
}

public class ExamListDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public int? ClassroomId { get; set; }
    public string? ClassroomName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ResultCount { get; set; }
}

public class ExamDetailDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public int? ClassroomId { get; set; }
    public string? ClassroomName { get; set; }
    public string AnswerKeyJson { get; set; } = "{}";
    public string LessonConfigJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public int ResultCount { get; set; }
    public int ConfirmedCount { get; set; }
    public bool IsConfirmed { get; set; }
}

public class ExamResultListDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public float TotalScore { get; set; }
    public float TotalNet { get; set; }
    public int TotalCorrect { get; set; }
    public int TotalWrong { get; set; }
    public int TotalEmpty { get; set; }
    public int? ClassRank { get; set; }
    public int? InstitutionRank { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}
