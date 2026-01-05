using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Operations;

public class InstitutionOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly CacheService _cacheService;
    private readonly AuditService _auditService;

    public InstitutionOperations(ApplicationContext context, SessionService sessionService, CacheService cacheService, AuditService auditService)
    {
        _context = context;
        _sessionService = sessionService;
        _cacheService = cacheService;
        _auditService = auditService;
    }

    public async Task<BaseResponse<bool>> AddUserToInstitutionAsync(int institutionId, int userId, InstitutionRole role, string? number = null)
    {
        var currentUserId = _sessionService.GetUserId();
        // Check if current user is admin or manager of this institution
        var isManager = await _context.Institutions.AnyAsync(i => i.Id == institutionId && i.ManagerUserId == currentUserId);

        if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
        {
            return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);
        }

        // Check if user already exists in institution
        var existing = await _context.InstitutionUsers.AnyAsync(iu => iu.UserId == userId && iu.InstitutionId == institutionId);
        if (existing)
        {
            return BaseResponse<bool>.ErrorResponse("User already in institution", "201001");
        }

        var institutionUser = new InstitutionUser
        {
            UserId = userId,
            InstitutionId = institutionId,
            Role = role,
            StudentNumber = role == InstitutionRole.Student ? number : null,
            EmployeeNumber = role != InstitutionRole.Student ? number : null,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        _context.InstitutionUsers.Add(institutionUser);
        await _context.SaveChangesAsync();

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<List<InstitutionUserDto>>> GetInstitutionUsersAsync(int institutionId, InstitutionRole? role = null)
    {
        var query = _context.InstitutionUsers
            .Include(iu => iu.User)
            .Where(iu => iu.InstitutionId == institutionId);

        if (role.HasValue)
        {
            query = query.Where(iu => iu.Role == role.Value);
        }

        var users = await query.Select(iu => new InstitutionUserDto
        {
            Id = iu.Id,
            UserId = iu.UserId,
            FullName = iu.User.FullName,
            Username = iu.User.Username,
            Role = iu.Role,
            Number = iu.Role == InstitutionRole.Student ? iu.StudentNumber : iu.EmployeeNumber,
            IsActive = iu.IsActive
        }).ToListAsync();

        return BaseResponse<List<InstitutionUserDto>>.SuccessResponse(users);
    }

    public async Task<BaseResponse<List<MyInstitutionDto>>> GetMyInstitutionsAsync(int userId)
    {
        var institutions = await _context.InstitutionUsers
            .Include(iu => iu.Institution)
            .Where(iu => iu.UserId == userId && iu.IsActive)
            .Select(iu => new MyInstitutionDto
            {
                Id = iu.InstitutionId,
                Name = iu.Institution.Name,
                Role = iu.Role.ToString(),
                Status = iu.Institution.Status.ToString(),
                JoinedAt = iu.JoinedAt
            })
            .ToListAsync();

        return BaseResponse<List<MyInstitutionDto>>.SuccessResponse(institutions);
    }

    public async Task<BaseResponse<InstitutionDetailResponseDto>> GetInstitutionAsync(int institutionId, int currentUserId, bool forceRefresh = false)
    {
        // Cache key
        var cacheKey = $"institution_detail_{institutionId}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<InstitutionDetailResponseDto>(cacheKey);
            if (cached != null)
                return BaseResponse<InstitutionDetailResponseDto>.SuccessResponse(cached);
        }

        // Authorization: User must be member of this institution or admin
        var hasAccess = await _context.InstitutionUsers.AnyAsync(iu =>
            iu.UserId == currentUserId &&
            iu.InstitutionId == institutionId);

        if (!hasAccess && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<InstitutionDetailResponseDto>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        var institution = await _context.Institutions
            .Include(i => i.Manager)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == institutionId);

        if (institution == null)
            return BaseResponse<InstitutionDetailResponseDto>.ErrorResponse("Institution not found", ErrorCodes.GenericError);

        var memberCount = await _context.InstitutionUsers.CountAsync(iu => iu.InstitutionId == institutionId && iu.IsActive);
        var studentCount = await _context.InstitutionUsers.CountAsync(iu => 
            iu.InstitutionId == institutionId && iu.Role == InstitutionRole.Student && iu.IsActive);
        var teacherCount = await _context.InstitutionUsers.CountAsync(iu => 
            iu.InstitutionId == institutionId && iu.Role == InstitutionRole.Teacher && iu.IsActive);
        var classroomCount = await _context.Classrooms.CountAsync(c => c.InstitutionId == institutionId);
        var examCount = await _context.Exams.CountAsync(e => e.InstitutionId == institutionId);

        var detail = new InstitutionDetailResponseDto
        {
            Id = institution.Id,
            Name = institution.Name,
            LicenseNumber = institution.LicenseNumber,
            Address = institution.Address,
            Phone = institution.Phone,
            ManagerName = institution.Manager.FullName,
            ManagerEmail = institution.Manager.Email,
            Status = institution.Status.ToString(),
            SubscriptionStartDate = institution.SubscriptionStartDate,
            SubscriptionEndDate = institution.SubscriptionEndDate,
            CreatedAt = institution.CreatedAt,
            ApprovedAt = institution.ApprovedAt,
            MemberCount = memberCount,
            StudentCount = studentCount,
            TeacherCount = teacherCount,
            ClassroomCount = classroomCount,
            ExamCount = examCount
        };

        // Cache for 5 minutes
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, detail, TimeSpan.FromMinutes(5));
        }

        return BaseResponse<InstitutionDetailResponseDto>.SuccessResponse(detail);
    }

    public async Task<BaseResponse<string>> UpdateInstitutionAsync(int institutionId, UpdateInstitutionRequest request, int userId)
    {
        // Authorization: Only manager can update
        var isManager = await _context.Institutions.AnyAsync(i => 
            i.Id == institutionId && i.ManagerUserId == userId);

        if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<string>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        var institution = await _context.Institutions.FindAsync(institutionId);
        if (institution == null)
            return BaseResponse<string>.ErrorResponse("Institution not found", ErrorCodes.GenericError);

        if (!string.IsNullOrWhiteSpace(request.Name))
            institution.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Address))
            institution.Address = request.Address;

        if (!string.IsNullOrWhiteSpace(request.Phone))
            institution.Phone = request.Phone;

        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync($"institution_detail_{institutionId}");
        await _cacheService.RemoveByPatternAsync("admin_institutions_*");

        await _auditService.LogAsync(userId, "InstitutionUpdated", 
            System.Text.Json.JsonSerializer.Serialize(new { InstitutionId = institutionId, Changes = request }));

        return BaseResponse<string>.SuccessResponse("Institution updated successfully");
    }

    public async Task<BaseResponse<PagedResult<InstitutionUserDto>>> GetMembersAsync(
        int institutionId,
        InstitutionRole? role = null,
        int page = 1,
        int limit = 20,
        string? search = null)
    {
        var query = _context.InstitutionUsers
            .Include(iu => iu.User)
            .AsNoTracking()
            .Where(iu => iu.InstitutionId == institutionId && iu.IsActive)
            .AsQueryable();

        if (role.HasValue)
            query = query.Where(iu => iu.Role == role.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(iu =>
                iu.User.FullName.Contains(search) ||
                iu.User.Username.Contains(search) ||
                iu.StudentNumber != null && iu.StudentNumber.Contains(search) ||
                iu.EmployeeNumber != null && iu.EmployeeNumber.Contains(search));
        }

        var total = await query.CountAsync();
        var users = await query
            .OrderBy(iu => iu.User.FullName)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(iu => new InstitutionUserDto
            {
                Id = iu.Id,
                UserId = iu.UserId,
                FullName = iu.User.FullName,
                Username = iu.User.Username,
                Role = iu.Role,
                Number = iu.Role == InstitutionRole.Student ? iu.StudentNumber : iu.EmployeeNumber,
                IsActive = iu.IsActive
            })
            .ToListAsync();

        var result = new PagedResult<InstitutionUserDto>
        {
            Items = users,
            Total = total,
            Page = page,
            Limit = limit,
            TotalPages = (int)Math.Ceiling(total / (double)limit)
        };

        return BaseResponse<PagedResult<InstitutionUserDto>>.SuccessResponse(result);
    }

    public async Task<BaseResponse<string>> AddMemberAsync(int institutionId, AddMemberRequest request, int userId)
    {
        // Authorization: Only manager can add members
        var isManager = await _context.Institutions.AnyAsync(i => 
            i.Id == institutionId && i.ManagerUserId == userId);

        if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<string>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        // Check if user already exists
        var existing = await _context.InstitutionUsers.AnyAsync(iu => 
            iu.UserId == request.UserId && iu.InstitutionId == institutionId);

        if (existing)
            return BaseResponse<string>.ErrorResponse("User already in institution", ErrorCodes.GenericError);

        var institutionUser = new InstitutionUser
        {
            UserId = request.UserId,
            InstitutionId = institutionId,
            Role = request.Role,
            StudentNumber = request.Role == InstitutionRole.Student ? request.Number : null,
            EmployeeNumber = request.Role != InstitutionRole.Student ? request.Number : null,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        _context.InstitutionUsers.Add(institutionUser);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync($"institution_detail_{institutionId}");

        await _auditService.LogAsync(userId, "MemberAdded", 
            System.Text.Json.JsonSerializer.Serialize(new { InstitutionId = institutionId, UserId = request.UserId, Role = request.Role }));

        return BaseResponse<string>.SuccessResponse("Member added successfully");
    }

    public async Task<BaseResponse<string>> RemoveMemberAsync(int institutionId, int memberId, int userId)
    {
        // Authorization: Only manager can remove members
        var isManager = await _context.Institutions.AnyAsync(i => 
            i.Id == institutionId && i.ManagerUserId == userId);

        if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<string>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        var member = await _context.InstitutionUsers
            .FirstOrDefaultAsync(iu => iu.Id == memberId && iu.InstitutionId == institutionId);

        if (member == null)
            return BaseResponse<string>.ErrorResponse("Member not found", ErrorCodes.GenericError);

        // Cannot remove manager
        if (member.Role == InstitutionRole.Manager)
            return BaseResponse<string>.ErrorResponse("Cannot remove manager", ErrorCodes.AccessDenied);

        member.IsActive = false;
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync($"institution_detail_{institutionId}");

        await _auditService.LogAsync(userId, "MemberRemoved", 
            System.Text.Json.JsonSerializer.Serialize(new { InstitutionId = institutionId, MemberId = memberId }));

        return BaseResponse<string>.SuccessResponse("Member removed successfully");
    }

    public async Task<BaseResponse<string>> UpdateMemberRoleAsync(int institutionId, int memberId, InstitutionRole newRole, int userId)
    {
        // Authorization: Only manager can update roles
        var isManager = await _context.Institutions.AnyAsync(i => 
            i.Id == institutionId && i.ManagerUserId == userId);

        if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<string>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        var member = await _context.InstitutionUsers
            .FirstOrDefaultAsync(iu => iu.Id == memberId && iu.InstitutionId == institutionId);

        if (member == null)
            return BaseResponse<string>.ErrorResponse("Member not found", ErrorCodes.GenericError);

        // Cannot change manager role
        if (member.Role == InstitutionRole.Manager || newRole == InstitutionRole.Manager)
            return BaseResponse<string>.ErrorResponse("Cannot change manager role", ErrorCodes.AccessDenied);

        member.Role = newRole;
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync($"institution_detail_{institutionId}");

        await _auditService.LogAsync(userId, "MemberRoleUpdated", 
            System.Text.Json.JsonSerializer.Serialize(new { InstitutionId = institutionId, MemberId = memberId, NewRole = newRole }));

        return BaseResponse<string>.SuccessResponse("Member role updated successfully");
    }

    public async Task<BaseResponse<InstitutionStatisticsDto>> GetStatisticsAsync(int institutionId, int currentUserId, bool forceRefresh = false)
    {
        // Cache key
        var cacheKey = $"institution_statistics_{institutionId}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<InstitutionStatisticsDto>(cacheKey);
            if (cached != null)
                return BaseResponse<InstitutionStatisticsDto>.SuccessResponse(cached);
        }

        // Authorization
        var hasAccess = await _context.InstitutionUsers.AnyAsync(iu =>
            iu.UserId == currentUserId &&
            iu.InstitutionId == institutionId &&
            (iu.Role == InstitutionRole.Manager || iu.Role == InstitutionRole.Teacher));

        if (!hasAccess && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<InstitutionStatisticsDto>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        var studentCount = await _context.InstitutionUsers.CountAsync(iu => 
            iu.InstitutionId == institutionId && iu.Role == InstitutionRole.Student && iu.IsActive);
        var teacherCount = await _context.InstitutionUsers.CountAsync(iu => 
            iu.InstitutionId == institutionId && iu.Role == InstitutionRole.Teacher && iu.IsActive);
        var classroomCount = await _context.Classrooms.CountAsync(c => c.InstitutionId == institutionId);
        var examCount = await _context.Exams.CountAsync(e => e.InstitutionId == institutionId);

        // Calculate average success rate
        var examResults = await _context.ExamResults
            .Include(er => er.Exam)
            .Where(er => er.Exam.InstitutionId == institutionId && er.IsConfirmed)
            .ToListAsync();

        var averageScore = examResults.Any() ? examResults.Average(er => er.TotalScore) : 0;
        var averageNet = examResults.Any() ? examResults.Average(er => er.TotalNet) : 0;

        var statistics = new InstitutionStatisticsDto
        {
            TotalStudents = studentCount,
            TotalTeachers = teacherCount,
            TotalClassrooms = classroomCount,
            TotalExams = examCount,
            AverageScore = averageScore,
            AverageNet = averageNet
        };

        // Cache for 10 minutes
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, statistics, TimeSpan.FromMinutes(10));
        }

        return BaseResponse<InstitutionStatisticsDto>.SuccessResponse(statistics);
    }
}

public class MyInstitutionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class InstitutionDetailResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string ManagerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int MemberCount { get; set; }
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int ClassroomCount { get; set; }
    public int ExamCount { get; set; }
}

public class InstitutionStatisticsDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClassrooms { get; set; }
    public int TotalExams { get; set; }
    public float AverageScore { get; set; }
    public float AverageNet { get; set; }
}

public class InstitutionUserDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string FullName { get; set; }
    public required string Username { get; set; }
    public InstitutionRole Role { get; set; }
    public string? Number { get; set; }
    public bool IsActive { get; set; }
}
