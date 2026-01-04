using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Operations;

public class ExamOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly OpticalParserService _opticalParserService;
    private readonly NotificationService _notificationService;

    public ExamOperations(ApplicationContext context, SessionService sessionService, OpticalParserService opticalParserService, NotificationService notificationService)
    {
        _context = context;
        _sessionService = sessionService;
        _opticalParserService = opticalParserService;
        _notificationService = notificationService;
    }

    public async Task<BaseResponse<int>> CreateExamAsync(CreateExamDto dto)
    {
        var userId = _sessionService.GetUserId();
        // Validation: Does user have authority in this institution?
        var canCreate = await _context.InstitutionUsers.AnyAsync(iu =>
            iu.InstitutionId == dto.InstitutionId &&
            iu.UserId == userId &&
            (iu.Role == InstitutionRole.Manager || iu.Role == InstitutionRole.Teacher));

        if (!canCreate && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
        {
            return BaseResponse<int>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
        }

        var exam = new Exam
        {
            InstitutionId = dto.InstitutionId,
            ClassroomId = dto.ClassroomId,
            Title = dto.Title,
            Type = dto.Type,
            ExamDate = dto.ExamDate,
            AnswerKeyJson = dto.AnswerKeyJson, // Expecting Dictionary<string, string>
            LessonConfigJson = dto.LessonConfigJson, // Expecting Dictionary<string, LessonConfig>
            CreatedAt = DateTime.UtcNow,
            Institution = null!,
            Classroom = null!
        };

        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();

        return BaseResponse<int>.SuccessResponse(exam.Id);
    }

    public async Task<BaseResponse<bool>> ProcessOpticalResultsAsync(int examId, Stream fileStream)
    {
        var exam = await _context.Exams.Include(e => e.Institution).FirstOrDefaultAsync(e => e.Id == examId);
        if (exam == null) return BaseResponse<bool>.ErrorResponse("Exam not found", ErrorCodes.GenericError);

        var parsedLines = await _opticalParserService.ParseFileAsync(fileStream);
        var answerKey = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(exam.AnswerKeyJson) ?? new();
        var lessonConfigs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, LessonConfig>>(exam.LessonConfigJson) ?? new();

        // Optimization: Bulk fetch students for this institution
        var studentNumbers = parsedLines.Select(l => l.StudentNumber).ToList();
        var students = await _context.InstitutionUsers
            .Where(iu => iu.InstitutionId == exam.InstitutionId && iu.Role == InstitutionRole.Student && studentNumbers.Contains(iu.StudentNumber!))
            .ToDictionaryAsync(iu => iu.StudentNumber!, iu => iu);

        foreach (var line in parsedLines)
        {
            if (!students.TryGetValue(line.StudentNumber, out var student)) continue;

            var lessonResults = _opticalParserService.CalculateResults(line.Answers, answerKey, lessonConfigs);

            var examResult = new ExamResult
            {
                ExamId = examId,
                StudentId = student.UserId, // FIX: Use UserId, not InstitutionUser.Id
                StudentNumber = line.StudentNumber,
                BookletType = line.BookletType,
                TotalCorrect = lessonResults.Values.Sum(r => r.Correct),
                TotalWrong = lessonResults.Values.Sum(r => r.Wrong),
                TotalEmpty = lessonResults.Values.Sum(r => r.Empty),
                TotalNet = lessonResults.Values.Sum(r => r.Net),
                DetailedResultsJson = System.Text.Json.JsonSerializer.Serialize(lessonResults),
                CreatedAt = DateTime.UtcNow,
                Exam = null!,
                Student = null!
            };

            _context.ExamResults.Add(examResult);
        }

        await _context.SaveChangesAsync();
        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> ConfirmResultsAndNotifyAsync(int examId)
    {
        var results = await _context.ExamResults
            .Where(er => er.ExamId == examId)
            .ToListAsync();

        if (!results.Any()) return BaseResponse<bool>.ErrorResponse("No results found", ErrorCodes.GenericError);

        // 1. Calculate Ranks
        // Institution Rank
        var orderedResults = results.OrderByDescending(r => r.TotalNet).ToList();
        for (int i = 0; i < orderedResults.Count; i++)
        {
            orderedResults[i].InstitutionRank = i + 1;
            orderedResults[i].IsConfirmed = true;
            orderedResults[i].ConfirmedAt = DateTime.UtcNow;
        }

        // 2. Class Rank
        // Get classroom mappings for these students
        var studentIds = results.Select(r => r.StudentId).ToList();
        var classroomMap = await _context.ClassroomStudents
            .Where(cs => studentIds.Contains(cs.Student.UserId))
            .Select(cs => new { cs.Student.UserId, cs.ClassroomId })
            .ToListAsync();

        var studentToClass = classroomMap.ToDictionary(x => x.UserId, x => x.ClassroomId);

        var resultsByClass = results.GroupBy(r => studentToClass.ContainsKey(r.StudentId) ? studentToClass[r.StudentId] : -1);

        foreach (var classGroup in resultsByClass)
        {
            if (classGroup.Key == -1) continue;
            var classOrdered = classGroup.OrderByDescending(r => r.TotalNet).ToList();
            for (int i = 0; i < classOrdered.Count; i++)
            {
                classOrdered[i].ClassRank = i + 1;
            }
        }

        await _context.SaveChangesAsync();

        // 2. Notify Students
        foreach (var result in results)
        {
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId); // Fetch exam details for notification
            if (exam == null) continue; // Skip if exam not found (shouldn't happen here)

            await _notificationService.SendNotificationAsync(
                result.StudentId,
                "Sınav Sonucunuz Açıklandı",
                $"{exam.Title} sınav sonucunuz onaylandı. Kurum Sırası: {result.InstitutionRank}, Sınıf Sırası: {result.ClassRank}",
                NotificationType.System,
                $"/exams/results/{result.Id}"
            );
        }

        return BaseResponse<bool>.SuccessResponse(true);
    }
}

public class CreateExamDto
{
    public int InstitutionId { get; set; }
    public int? ClassroomId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ExamType Type { get; set; }
    public DateTime ExamDate { get; set; }
    public string AnswerKeyJson { get; set; } = "{}";
    public string LessonConfigJson { get; set; } = "{}";
}
