using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Responses;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KeremProject1backend.Operations;

public class ClassroomOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly AuditService _auditService;
    private readonly CacheService _cacheService;
    private readonly AuthorizationService _authorizationService;

    public ClassroomOperations(
        ApplicationContext context,
        SessionService sessionService,
        AuditService auditService,
        CacheService cacheService,
        AuthorizationService authorizationService)
    {
        _context = context;
        _sessionService = sessionService;
        _auditService = auditService;
        _cacheService = cacheService;
        _authorizationService = authorizationService;
    }

    public async Task<BaseResponse<int>> CreateClassroomAsync(int institutionId, string name, int grade)
    {
        // 1. YETKİ KONTROLÜ
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.Manager,
            UserRole.AdminAdmin,
            UserRole.Admin);
        if (authError != null)
            return BaseResponse<int>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        // 2. Validation
        if (string.IsNullOrWhiteSpace(name))
            return BaseResponse<int>.ErrorResponse("Sınıf adı gereklidir", ErrorCodes.ValidationFailed);

        if (grade < 1 || grade > 12)
            return BaseResponse<int>.ErrorResponse("Geçersiz sınıf seviyesi", ErrorCodes.ValidationFailed);

        var userId = _sessionService.GetUserId();

        var classroom = new Classroom
        {
            InstitutionId = institutionId,
            Name = name,
            Grade = grade,
            CreatedAt = DateTime.UtcNow,
            Institution = null!
        };

        _context.Classrooms.Add(classroom);
        await _context.SaveChangesAsync();

        // Automatic Conversation Creation
        var conversation = new Conversation
        {
            InstitutionId = institutionId,
            ClassroomId = classroom.Id,
            Title = $"{name} Sınıf Grubu",
            IsGroup = true,
            CreatedAt = DateTime.UtcNow,
            Institution = null!
        };
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(userId, "ClassroomCreated", JsonSerializer.Serialize(new { ClassroomId = classroom.Id, Name = name }));

        // Cache invalidation: Remove institution classrooms cache
        await _cacheService.RemoveByPatternAsync($"Inst:{institutionId}:Classrooms");

        return BaseResponse<int>.SuccessResponse(classroom.Id);
    }

    public async Task<BaseResponse<bool>> AddStudentToClassroomAsync(int classroomId, int studentInstitutionUserId)
    {
        // 1. YETKİ KONTROLÜ
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.Teacher,
            UserRole.StandaloneTeacher,
            UserRole.Manager,
            UserRole.AdminAdmin,
            UserRole.Admin);
        if (authError != null)
            return BaseResponse<bool>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        var userId = _sessionService.GetUserId();
        // studentInstitutionUserId is the ID from InstitutionUsers table
        var classroom = await _context.Classrooms.FindAsync(classroomId);
        if (classroom == null) return BaseResponse<bool>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        var existing = await _context.ClassroomStudents.AnyAsync(cs =>
            cs.ClassroomId == classroomId &&
            cs.InstitutionUserId == studentInstitutionUserId &&
            cs.RemovedAt == null);
        if (existing) return BaseResponse<bool>.ErrorResponse("Student already in class", "202001");

        var cs = new ClassroomStudent
        {
            ClassroomId = classroomId,
            InstitutionUserId = studentInstitutionUserId,
            AssignedAt = DateTime.UtcNow,
            Classroom = null!,
            Student = null!
        };

        _context.ClassroomStudents.Add(cs);
        await _context.SaveChangesAsync();

        if (classroom != null)
        {
            await _auditService.LogAsync(userId, "StudentAddedToClassroom", JsonSerializer.Serialize(new { ClassroomId = classroomId, StudentInstitutionUserId = studentInstitutionUserId }));

            // Cache invalidation: Remove classroom details cache
            await _cacheService.RemoveByPatternAsync($"Classroom:{classroomId}:Details");
            await _cacheService.RemoveByPatternAsync($"Inst:{classroom.InstitutionId}:Classrooms");
        }

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> AddStudentsToClassroomAsync(int classroomId, List<int> studentInstitutionUserIds)
    {
        // 1. YETKİ KONTROLÜ
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.Teacher,
            UserRole.StandaloneTeacher,
            UserRole.Manager,
            UserRole.AdminAdmin,
            UserRole.Admin);
        if (authError != null)
            return BaseResponse<bool>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        var userId = _sessionService.GetUserId();
        var classroom = await _context.Classrooms.FindAsync(classroomId);
        if (classroom == null) return BaseResponse<bool>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        var students = new List<ClassroomStudent>();
        foreach (var id in studentInstitutionUserIds)
        {
            var existing = await _context.ClassroomStudents.AnyAsync(cs =>
                cs.ClassroomId == classroomId &&
                cs.InstitutionUserId == id &&
                cs.RemovedAt == null);
            if (existing) continue;

            students.Add(new ClassroomStudent
            {
                ClassroomId = classroomId,
                InstitutionUserId = id,
                AssignedAt = DateTime.UtcNow,
                Classroom = null!,
                Student = null!
            });
        }

        if (students.Any())
        {
            _context.ClassroomStudents.AddRange(students);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, "StudentsBulkAddedToClassroom", JsonSerializer.Serialize(new { ClassroomId = classroomId, Count = students.Count }));

            // Cache invalidation
            await _cacheService.RemoveByPatternAsync($"Classroom:{classroomId}:Details");
            await _cacheService.RemoveByPatternAsync($"Inst:{classroom.InstitutionId}:Classrooms");
        }

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<ClassroomDetailDto>> GetClassroomDetailsAsync(int classroomId, bool forceRefresh = false)
    {
        // Check cache first
        var cacheKey = $"Classroom:{classroomId}:Details";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<ClassroomDetailDto>(cacheKey);
            if (cached != null)
            {
                return BaseResponse<ClassroomDetailDto>.SuccessResponse(cached);
            }
        }

        // Fetch with join for better mapping
        var classroom = await _context.Classrooms
            .Include(c => c.Students)
                .ThenInclude(cs => cs.Student)
                    .ThenInclude(iu => iu.User)
            .FirstOrDefaultAsync(c => c.Id == classroomId);

        if (classroom == null) return BaseResponse<ClassroomDetailDto>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        var dto = new ClassroomDetailDto
        {
            Id = classroom.Id,
            Name = classroom.Name,
            Grade = classroom.Grade,
            InstitutionId = classroom.InstitutionId,
            Students = classroom.Students
                .Where(cs => cs.RemovedAt == null)
                .Select(cs => new ClassroomStudentDto
                {
                    InstitutionUserId = cs.InstitutionUserId,
                    FullName = cs.Student.User.FullName,
                    StudentNumber = cs.Student.StudentNumber,
                    AssignedAt = cs.AssignedAt
                }).ToList()
        };

        // Cache for 15 minutes
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15));
        }

        return BaseResponse<ClassroomDetailDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<List<ClassroomDto>>> GetClassroomsAsync(int institutionId, bool forceRefresh = false)
    {
        // Check cache first
        var cacheKey = $"Inst:{institutionId}:Classrooms";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<ClassroomDto>>(cacheKey);
            if (cached != null)
            {
                return BaseResponse<List<ClassroomDto>>.SuccessResponse(cached);
            }
        }

        var classrooms = await _context.Classrooms
            .Where(c => c.InstitutionId == institutionId)
            .Select(c => new ClassroomDto
            {
                Id = c.Id,
                Name = c.Name,
                Grade = c.Grade,
                StudentCount = c.Students.Count(cs => cs.RemovedAt == null)
            }).ToListAsync();

        // Cache for 30 minutes
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, classrooms, TimeSpan.FromMinutes(30));
        }

        return BaseResponse<List<ClassroomDto>>.SuccessResponse(classrooms);
    }

    public async Task<BaseResponse<string>> UpdateClassroomAsync(int classroomId, UpdateClassroomRequest request, int userId)
    {
        // 1. YETKİ KONTROLÜ
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.Manager,
            UserRole.AdminAdmin,
            UserRole.Admin);
        if (authError != null)
            return BaseResponse<string>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        var classroom = await _context.Classrooms.FindAsync(classroomId);
        if (classroom == null)
            return BaseResponse<string>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        if (!string.IsNullOrWhiteSpace(request.Name))
            classroom.Name = request.Name;

        if (request.Grade.HasValue)
            classroom.Grade = request.Grade.Value;

        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync($"Classroom:{classroomId}:Details");
        await _cacheService.RemoveByPatternAsync($"Inst:{classroom.InstitutionId}:Classrooms");

        return BaseResponse<string>.SuccessResponse("Classroom updated successfully");
    }

    public async Task<BaseResponse<string>> DeleteClassroomAsync(int classroomId, int userId)
    {
        // 1. YETKİ KONTROLÜ
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.Manager,
            UserRole.AdminAdmin,
            UserRole.Admin);
        if (authError != null)
            return BaseResponse<string>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        var classroom = await _context.Classrooms.FindAsync(classroomId);
        if (classroom == null)
            return BaseResponse<string>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        // Soft delete
        // Note: If Classroom has IsActive field, use it. Otherwise, we might need to add it.
        // For now, we'll keep the classroom but mark it as inactive if there's such a field
        // If not, we can remove it (but this might break relationships)
        // Let's check if there's an IsActive field first
        _context.Classrooms.Remove(classroom); // Hard delete for now, can be changed to soft delete
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync($"Classroom:{classroomId}:Details");
        await _cacheService.RemoveByPatternAsync($"Inst:{classroom.InstitutionId}:Classrooms");

        return BaseResponse<string>.SuccessResponse("Classroom deleted successfully");
    }

    public async Task<BaseResponse<string>> RemoveStudentAsync(int classroomId, int studentId, int userId)
    {
        // 1. YETKİ KONTROLÜ
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.Teacher,
            UserRole.StandaloneTeacher,
            UserRole.Manager,
            UserRole.AdminAdmin,
            UserRole.Admin);
        if (authError != null)
            return BaseResponse<string>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        var classroom = await _context.Classrooms.FindAsync(classroomId);
        if (classroom == null)
            return BaseResponse<string>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        var classroomStudent = await _context.ClassroomStudents
            .FirstOrDefaultAsync(cs => cs.ClassroomId == classroomId && cs.InstitutionUserId == studentId && cs.RemovedAt == null);

        if (classroomStudent == null)
            return BaseResponse<string>.ErrorResponse("Student not found in classroom", ErrorCodes.GenericError);

        // Soft delete
        classroomStudent.RemovedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync($"Classroom:{classroomId}:Details");
        await _cacheService.RemoveByPatternAsync($"Inst:{classroom.InstitutionId}:Classrooms");

        return BaseResponse<string>.SuccessResponse("Student removed from classroom successfully");
    }

    public async Task<BaseResponse<List<ClassroomStudentDto>>> GetStudentsAsync(int classroomId, int page = 1, int limit = 50, string? search = null)
    {
        var query = _context.ClassroomStudents
            .Include(cs => cs.Student)
                .ThenInclude(s => s.User)
            .Where(cs => cs.ClassroomId == classroomId && cs.RemovedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(cs =>
                cs.Student.User.FullName.Contains(search) ||
                cs.Student.StudentNumber != null && cs.Student.StudentNumber.Contains(search));
        }

        var students = await query
            .OrderBy(cs => cs.Student.User.FullName)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(cs => new ClassroomStudentDto
            {
                InstitutionUserId = cs.InstitutionUserId,
                FullName = cs.Student.User.FullName,
                StudentNumber = cs.Student.StudentNumber,
                AssignedAt = cs.AssignedAt
            })
            .ToListAsync();

        return BaseResponse<List<ClassroomStudentDto>>.SuccessResponse(students);
    }
}

public class UpdateClassroomRequest
{
    public string? Name { get; set; }
    public int? Grade { get; set; }
}

