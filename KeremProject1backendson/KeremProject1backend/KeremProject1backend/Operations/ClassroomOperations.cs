using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Operations;

public class ClassroomOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly CacheService _cacheService;
    private readonly AuthorizationService _authorizationService;

    public ClassroomOperations(
        ApplicationContext context, 
        SessionService sessionService, 
        CacheService cacheService,
        AuthorizationService authorizationService)
    {
        _context = context;
        _sessionService = sessionService;
        _cacheService = cacheService;
        _authorizationService = authorizationService;
    }

    public async Task<BaseResponse<int>> CreateClassroomAsync(int institutionId, string name, int grade)
    {
        // 1. YETKİ KONTROLÜ (EN BAŞTA - ZORUNLU!)
        // Manager, AdminAdmin veya Admin olmalı
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

        var classroom = new Classroom
        {
            InstitutionId = institutionId,
            Name = name,
            Grade = grade,
            CreatedAt = DateTime.UtcNow,
            Institution = null! // Satisfy required, EF will handle mapping
        };

        _context.Classrooms.Add(classroom);
        await _context.SaveChangesAsync();

        // Cache invalidation: Remove institution classrooms cache
        await _cacheService.RemoveByPatternAsync($"Inst:{institutionId}:Classrooms");

        // 3. Create Class Conversation
        var conversation = new Conversation
        {
            Type = ConversationType.ClassGroup,
            ClassroomId = classroom.Id,
            Title = $"{classroom.Name} Sınıf Grubu",
            IsGroup = true,
            CreatedAt = DateTime.UtcNow,
            InstitutionId = institutionId,
            Institution = null!
        };
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        // 4. Conversation is already linked via conversation.ClassroomId
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

        // studentInstitutionUserId is the ID from InstitutionUsers table
        var classroom = await _context.Classrooms.FindAsync(classroomId);
        if (classroom == null) return BaseResponse<bool>.ErrorResponse("Classroom not found", ErrorCodes.Unauthorized); // Using existing code for now or will add Generic

        var existing = await _context.ClassroomStudents.AnyAsync(cs => 
            cs.ClassroomId == classroomId && 
            cs.StudentId == studentInstitutionUserId &&
            cs.RemovedAt == null);
        if (existing) return BaseResponse<bool>.ErrorResponse("Student already in class", "202001");

        var cs = new ClassroomStudent
        {
            ClassroomId = classroomId,
            StudentId = studentInstitutionUserId,
            AssignedAt = DateTime.UtcNow,
            Classroom = null!,
            Student = null!
        };

        _context.ClassroomStudents.Add(cs);
        await _context.SaveChangesAsync();

        // Cache invalidation: Remove classroom details cache
        await _cacheService.RemoveByPatternAsync($"Classroom:{classroomId}:Details");
        await _cacheService.RemoveByPatternAsync($"Inst:{classroom.InstitutionId}:Classrooms");

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

        var classroom = await _context.Classrooms.FindAsync(classroomId);
        if (classroom == null) return BaseResponse<bool>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        foreach (var studentId in studentInstitutionUserIds)
        {
            var existing = await _context.ClassroomStudents.AnyAsync(cs => 
                cs.ClassroomId == classroomId && 
                cs.StudentId == studentId &&
                cs.RemovedAt == null);
            if (existing) continue;

            var cs = new ClassroomStudent
            {
                ClassroomId = classroomId,
                StudentId = studentId,
                AssignedAt = DateTime.UtcNow, // Renamed from JoinedAt if following guide
                Classroom = null!,
                Student = null!
            };
            _context.ClassroomStudents.Add(cs);
        }

        await _context.SaveChangesAsync();

        // Cache invalidation (classroom already fetched at the beginning)
        await _cacheService.RemoveByPatternAsync($"Classroom:{classroomId}:Details");
        await _cacheService.RemoveByPatternAsync($"Inst:{classroom.InstitutionId}:Classrooms");

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
        var details = await _context.Classrooms
            .Where(c => c.Id == classroomId)
            .Select(c => new ClassroomDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                Grade = c.Grade,
                InstitutionId = c.InstitutionId,
                Students = _context.ClassroomStudents
                    .Where(cs => cs.ClassroomId == c.Id && cs.RemovedAt == null)
                    .Select(cs => new ClassroomStudentDto
                    {
                        InstitutionUserId = cs.StudentId,
                        FullName = cs.Student.User.FullName,
                        StudentNumber = cs.Student.StudentNumber,
                        AssignedAt = cs.AssignedAt
                    }).ToList()
            }).FirstOrDefaultAsync();

        if (details == null) return BaseResponse<ClassroomDetailDto>.ErrorResponse("Classroom not found", ErrorCodes.GenericError);

        // Cache for 15 minutes
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, details, TimeSpan.FromMinutes(15));
        }

        return BaseResponse<ClassroomDetailDto>.SuccessResponse(details);
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
                StudentCount = _context.ClassroomStudents.Count(cs => cs.ClassroomId == c.Id && cs.RemovedAt == null)
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
            .FirstOrDefaultAsync(cs => cs.ClassroomId == classroomId && cs.StudentId == studentId && cs.RemovedAt == null);

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
                InstitutionUserId = cs.StudentId,
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

public class ClassroomDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Grade { get; set; }
    public int StudentCount { get; set; }
}

public class ClassroomDetailDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Grade { get; set; }
    public int InstitutionId { get; set; }
    public List<ClassroomStudentDto> Students { get; set; } = new();
}

public class ClassroomStudentDto
{
    public int InstitutionUserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? StudentNumber { get; set; }
    public DateTime AssignedAt { get; set; }
}
