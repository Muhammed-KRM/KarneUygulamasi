using KeremProject1backend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.DTOs.Requests;

public class CreateExamRequest
{
    public int InstitutionId { get; set; }
    public int? ClassroomId { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public ExamType Type { get; set; }
    public DateTime ExamDate { get; set; }
    public string AnswerKeyJson { get; set; } = "{}";
    public string LessonConfigJson { get; set; } = "{}";
}

public class ConfirmResultsRequest
{
    public int ExamId { get; set; }
}
