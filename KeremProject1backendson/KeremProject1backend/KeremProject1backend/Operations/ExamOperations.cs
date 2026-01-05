using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using Hangfire;

namespace KeremProject1backend.Operations;

public class ExamOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly OpticalParserService _opticalParserService;
    private readonly NotificationService _notificationService;
    private readonly CacheService _cacheService;

    public ExamOperations(ApplicationContext context, SessionService sessionService, OpticalParserService opticalParserService, NotificationService notificationService, CacheService cacheService)
    {
        _context = context;
        _sessionService = sessionService;
        _opticalParserService = opticalParserService;
        _notificationService = notificationService;
        _cacheService = cacheService;
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

        // Invalidate exam list caches
        await _cacheService.InvalidateExamCacheAsync();
        await _cacheService.RemoveByPatternAsync("exam_detail_*");

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

        // Queue background job to calculate rankings
        BackgroundJob.Enqueue<KeremProject1backend.Jobs.CalculateRankingsJob>(job => job.Execute(examId));

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

        // Invalidate exam result caches
        await _cacheService.RemoveByPatternAsync($"exam_results_{examId}_*");
        await _cacheService.RemoveByPatternAsync($"exam_detail_{examId}");

        // Queue background job to send bulk notifications
        BackgroundJob.Enqueue<KeremProject1backend.Jobs.BulkNotificationJob>(job => job.Execute(examId));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<StudentReportDto>> GetStudentReportAsync(int resultId, int currentUserId)
    {
        var result = await _context.ExamResults
            .Include(er => er.Exam)
            .Include(er => er.Student)
            .FirstOrDefaultAsync(er => er.Id == resultId);

        if (result == null)
            return BaseResponse<StudentReportDto>.ErrorResponse("Karne bulunamadı", ErrorCodes.GenericError);

        // Security: Student can only see their own report, or teacher/admin can see any
        var isOwner = result.StudentId == currentUserId;
        var isTeacher = await _context.InstitutionUsers.AnyAsync(iu =>
            iu.UserId == currentUserId &&
            iu.InstitutionId == result.Exam.InstitutionId &&
            (iu.Role == InstitutionRole.Teacher || iu.Role == InstitutionRole.Manager));

        if (!isOwner && !isTeacher && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<StudentReportDto>.ErrorResponse("Yetkiniz yok", ErrorCodes.AccessDenied);

        // Parse detailed results
        var detailedResults = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, LessonScore>>(result.DetailedResultsJson) ?? new();

        var report = new StudentReportDto
        {
            ExamId = result.ExamId,
            ExamName = result.Exam.Title,
            ExamDate = result.Exam.ExamDate,
            StudentId = result.StudentId,
            StudentName = result.Student.FullName,
            StudentNumber = result.StudentNumber,
            TotalScore = result.TotalScore,
            TotalNet = result.TotalNet,
            TotalCorrect = result.TotalCorrect,
            TotalWrong = result.TotalWrong,
            TotalEmpty = result.TotalEmpty,
            ClassRank = result.ClassRank,
            InstitutionRank = result.InstitutionRank,
            IsConfirmed = result.IsConfirmed,
            Lessons = detailedResults.Select(kvp => new LessonReportDto
            {
                LessonName = kvp.Key,
                Correct = kvp.Value.Correct,
                Wrong = kvp.Value.Wrong,
                Empty = kvp.Value.Empty,
                Net = kvp.Value.Net,
                SuccessRate = kvp.Value.SuccessRate,
                TopicScores = kvp.Value.TopicScores ?? new List<TopicScore>()
            }).ToList()
        };

        return BaseResponse<StudentReportDto>.SuccessResponse(report);
    }

    public async Task<BaseResponse<List<ExamListDto>>> GetExamsAsync(
        int? institutionId = null,
        int? classroomId = null,
        ExamType? type = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        // Cache key: exam list with filters
        var cacheKey = $"exams_{institutionId}_{classroomId}_{type}_{dateFrom?.ToString("yyyyMMdd")}_{dateTo?.ToString("yyyyMMdd")}_{page}_{limit}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<ExamListDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<List<ExamListDto>>.SuccessResponse(cached);
        }

        var userId = _sessionService.GetUserId();
        var query = _context.Exams
            .Include(e => e.Institution)
            .Include(e => e.Classroom)
            .AsNoTracking() // Read-only, optimize
            .AsQueryable();

        // Authorization: User can only see exams from their institutions
        if (!_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
        {
            var userInstitutionIds = await _context.InstitutionUsers
                .Where(iu => iu.UserId == userId)
                .Select(iu => iu.InstitutionId)
                .ToListAsync();
            query = query.Where(e => userInstitutionIds.Contains(e.InstitutionId));
        }

        if (institutionId.HasValue)
            query = query.Where(e => e.InstitutionId == institutionId.Value);

        if (classroomId.HasValue)
            query = query.Where(e => e.ClassroomId == classroomId.Value);

        if (type.HasValue)
            query = query.Where(e => e.Type == type.Value);

        if (dateFrom.HasValue)
            query = query.Where(e => e.ExamDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(e => e.ExamDate <= dateTo.Value);

        var exams = await query
            .OrderByDescending(e => e.ExamDate)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(e => new ExamListDto
            {
                Id = e.Id,
                Title = e.Title,
                Type = e.Type.ToString(),
                ExamDate = e.ExamDate,
                InstitutionId = e.InstitutionId,
                InstitutionName = e.Institution.Name,
                ClassroomId = e.ClassroomId,
                ClassroomName = e.Classroom != null ? e.Classroom.Name : null,
                CreatedAt = e.CreatedAt,
                ResultCount = _context.ExamResults.Count(er => er.ExamId == e.Id)
            })
            .ToListAsync();

        // Cache for 2 minutes (exam lists change when new exams are created)
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, exams, TimeSpan.FromMinutes(2));
        }

        return BaseResponse<List<ExamListDto>>.SuccessResponse(exams);
    }

    public async Task<BaseResponse<ExamDetailDto>> GetExamAsync(int examId, bool forceRefresh = false)
    {
        // Cache key: exam detail
        var cacheKey = $"exam_detail_{examId}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<ExamDetailDto>(cacheKey);
            if (cached != null)
                return BaseResponse<ExamDetailDto>.SuccessResponse(cached);
        }

        var userId = _sessionService.GetUserId();
        var exam = await _context.Exams
            .Include(e => e.Institution)
            .Include(e => e.Classroom)
            .AsNoTracking() // Read-only
            .FirstOrDefaultAsync(e => e.Id == examId);

        if (exam == null)
            return BaseResponse<ExamDetailDto>.ErrorResponse("Exam not found", ErrorCodes.GenericError);

        // Authorization
        if (!_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
        {
            var hasAccess = await _context.InstitutionUsers.AnyAsync(iu =>
                iu.UserId == userId &&
                iu.InstitutionId == exam.InstitutionId);
            if (!hasAccess)
                return BaseResponse<ExamDetailDto>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);
        }

        var resultCount = await _context.ExamResults.CountAsync(er => er.ExamId == examId);
        var confirmedCount = await _context.ExamResults.CountAsync(er => er.ExamId == examId && er.IsConfirmed);

        var detail = new ExamDetailDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Type = exam.Type.ToString(),
            ExamDate = exam.ExamDate,
            InstitutionId = exam.InstitutionId,
            InstitutionName = exam.Institution.Name,
            ClassroomId = exam.ClassroomId,
            ClassroomName = exam.Classroom?.Name,
            AnswerKeyJson = exam.AnswerKeyJson,
            LessonConfigJson = exam.LessonConfigJson,
            CreatedAt = exam.CreatedAt,
            ResultCount = resultCount,
            ConfirmedCount = confirmedCount,
            IsConfirmed = confirmedCount > 0 && confirmedCount == resultCount
        };

        // Cache for 5 minutes
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, detail, TimeSpan.FromMinutes(5));
        }

        return BaseResponse<ExamDetailDto>.SuccessResponse(detail);
    }

    public async Task<BaseResponse<List<ExamResultListDto>>> GetExamResultsAsync(
        int examId,
        int? studentId = null,
        int? classroomId = null,
        bool? isConfirmed = null,
        int page = 1,
        int limit = 50,
        bool forceRefresh = false)
    {
        // Cache key: exam results with filters
        var cacheKey = $"exam_results_{examId}_{studentId}_{classroomId}_{isConfirmed}_{page}_{limit}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<ExamResultListDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<List<ExamResultListDto>>.SuccessResponse(cached);
        }
        var userId = _sessionService.GetUserId();
        var exam = await _context.Exams.FindAsync(examId);
        if (exam == null)
            return BaseResponse<List<ExamResultListDto>>.ErrorResponse("Exam not found", ErrorCodes.GenericError);

        // Authorization
        var isStudent = await _context.InstitutionUsers.AnyAsync(iu =>
            iu.UserId == userId &&
            iu.InstitutionId == exam.InstitutionId &&
            iu.Role == InstitutionRole.Student);

        if (isStudent && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
        {
            // Students can only see their own results
            studentId = userId;
        }

        var query = _context.ExamResults
            .Include(er => er.Student)
            .AsNoTracking() // Read-only
            .Where(er => er.ExamId == examId)
            .AsQueryable();

        if (studentId.HasValue)
            query = query.Where(er => er.StudentId == studentId.Value);

        if (isConfirmed.HasValue)
            query = query.Where(er => er.IsConfirmed == isConfirmed.Value);

        if (classroomId.HasValue)
        {
            var studentIdsInClass = await _context.ClassroomStudents
                .Where(cs => cs.ClassroomId == classroomId.Value)
                .Select(cs => cs.Student.UserId)
                .ToListAsync();
            query = query.Where(er => studentIdsInClass.Contains(er.StudentId));
        }

        var results = await query
            .OrderByDescending(er => er.TotalNet)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(er => new ExamResultListDto
            {
                Id = er.Id,
                StudentId = er.StudentId,
                StudentName = er.Student.FullName,
                StudentNumber = er.StudentNumber,
                TotalScore = er.TotalScore,
                TotalNet = er.TotalNet,
                TotalCorrect = er.TotalCorrect,
                TotalWrong = er.TotalWrong,
                TotalEmpty = er.TotalEmpty,
                ClassRank = er.ClassRank,
                InstitutionRank = er.InstitutionRank,
                IsConfirmed = er.IsConfirmed,
                CreatedAt = er.CreatedAt
            })
            .ToListAsync();

        // Cache for 2 minutes (results change when confirmed)
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(2));
        }

        return BaseResponse<List<ExamResultListDto>>.SuccessResponse(results);
    }

    public async Task<BaseResponse<List<StudentReportDto>>> GetStudentAllReportsAsync(int studentId, int currentUserId)
    {
        // Authorization: Student can only see their own reports, or teacher/admin can see any
        var isOwner = studentId == currentUserId;
        var isTeacher = await _context.InstitutionUsers.AnyAsync(iu =>
            iu.UserId == currentUserId &&
            iu.Role == InstitutionRole.Teacher);

        if (!isOwner && !isTeacher && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<List<StudentReportDto>>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        var results = await _context.ExamResults
            .Include(er => er.Exam)
            .Include(er => er.Student)
            .Where(er => er.StudentId == studentId && er.IsConfirmed)
            .OrderByDescending(er => er.Exam.ExamDate)
            .ToListAsync();

        var reports = new List<StudentReportDto>();
        foreach (var result in results)
        {
            var detailedResults = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, LessonScore>>(result.DetailedResultsJson) ?? new();
            reports.Add(new StudentReportDto
            {
                ExamId = result.ExamId,
                ExamName = result.Exam.Title,
                ExamDate = result.Exam.ExamDate,
                StudentId = result.StudentId,
                StudentName = result.Student.FullName,
                StudentNumber = result.StudentNumber,
                TotalScore = result.TotalScore,
                TotalNet = result.TotalNet,
                TotalCorrect = result.TotalCorrect,
                TotalWrong = result.TotalWrong,
                TotalEmpty = result.TotalEmpty,
                ClassRank = result.ClassRank,
                InstitutionRank = result.InstitutionRank,
                IsConfirmed = result.IsConfirmed,
                Lessons = detailedResults.Select(kvp => new LessonReportDto
                {
                    LessonName = kvp.Key,
                    Correct = kvp.Value.Correct,
                    Wrong = kvp.Value.Wrong,
                    Empty = kvp.Value.Empty,
                    Net = kvp.Value.Net,
                    SuccessRate = kvp.Value.SuccessRate,
                    TopicScores = kvp.Value.TopicScores ?? new List<TopicScore>()
                }).ToList()
            });
        }

        return BaseResponse<List<StudentReportDto>>.SuccessResponse(reports);
    }

    public async Task<BaseResponse<ClassroomReportDto>> GetClassroomReportsAsync(int classroomId, int? examId = null, int currentUserId = 0)
    {
        // Authorization: Only teachers/managers can see classroom reports
        var classroom = await _context.Classrooms
            .Include(c => c.Institution)
            .FirstOrDefaultAsync(c => c.Id == classroomId);

        if (classroom == null)
            return BaseResponse<ClassroomReportDto>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        var hasAccess = await _context.InstitutionUsers.AnyAsync(iu =>
            iu.UserId == currentUserId &&
            iu.InstitutionId == classroom.InstitutionId &&
            (iu.Role == InstitutionRole.Teacher || iu.Role == InstitutionRole.Manager));

        if (!hasAccess && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<ClassroomReportDto>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        var query = _context.ExamResults
            .Include(er => er.Exam)
            .Include(er => er.Student)
            .Where(er => er.IsConfirmed);

        if (examId.HasValue)
        {
            query = query.Where(er => er.ExamId == examId.Value);
        }

        // Get students in this classroom
        var studentIds = await _context.ClassroomStudents
            .Where(cs => cs.ClassroomId == classroomId)
            .Select(cs => cs.Student.UserId)
            .ToListAsync();

        query = query.Where(er => studentIds.Contains(er.StudentId));

        var results = await query
            .OrderByDescending(er => er.TotalNet)
            .ToListAsync();

        var studentReports = new List<StudentReportDto>();
        foreach (var result in results)
        {
            var detailedResults = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, LessonScore>>(result.DetailedResultsJson) ?? new();
            studentReports.Add(new StudentReportDto
            {
                ExamId = result.ExamId,
                ExamName = result.Exam.Title,
                ExamDate = result.Exam.ExamDate,
                StudentId = result.StudentId,
                StudentName = result.Student.FullName,
                StudentNumber = result.StudentNumber,
                TotalScore = result.TotalScore,
                TotalNet = result.TotalNet,
                TotalCorrect = result.TotalCorrect,
                TotalWrong = result.TotalWrong,
                TotalEmpty = result.TotalEmpty,
                ClassRank = result.ClassRank,
                InstitutionRank = result.InstitutionRank,
                IsConfirmed = result.IsConfirmed,
                Lessons = detailedResults.Select(kvp => new LessonReportDto
                {
                    LessonName = kvp.Key,
                    Correct = kvp.Value.Correct,
                    Wrong = kvp.Value.Wrong,
                    Empty = kvp.Value.Empty,
                    Net = kvp.Value.Net,
                    SuccessRate = kvp.Value.SuccessRate,
                    TopicScores = kvp.Value.TopicScores ?? new List<TopicScore>()
                }).ToList()
            });
        }

        var classroomReport = new ClassroomReportDto
        {
            ClassroomId = classroomId,
            ClassroomName = classroom.Name,
            ExamId = examId,
            ExamName = examId.HasValue ? results.FirstOrDefault()?.Exam.Title : null,
            StudentCount = studentIds.Count,
            ResultCount = results.Count,
            AverageScore = results.Any() ? results.Average(r => r.TotalScore) : 0,
            AverageNet = results.Any() ? results.Average(r => r.TotalNet) : 0,
            Students = studentReports
        };

        return BaseResponse<ClassroomReportDto>.SuccessResponse(classroomReport);
    }
}

public class ClassroomReportDto
{
    public int ClassroomId { get; set; }
    public string ClassroomName { get; set; } = string.Empty;
    public int? ExamId { get; set; }
    public string? ExamName { get; set; }
    public int StudentCount { get; set; }
    public int ResultCount { get; set; }
    public float AverageScore { get; set; }
    public float AverageNet { get; set; }
    public List<StudentReportDto> Students { get; set; } = new();
}

public class StudentReportDto
{
    public int ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
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
    public List<LessonReportDto> Lessons { get; set; } = new();
}

public class LessonReportDto
{
    public string LessonName { get; set; } = string.Empty;
    public int Correct { get; set; }
    public int Wrong { get; set; }
    public int Empty { get; set; }
    public float Net { get; set; }
    public int SuccessRate { get; set; }
    public List<TopicScore> TopicScores { get; set; } = new();
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

public class ExamListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
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
    public string Title { get; set; } = string.Empty;
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
