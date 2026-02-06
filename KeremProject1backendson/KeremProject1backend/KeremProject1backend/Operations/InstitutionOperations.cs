using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using KeremProject1backend.Core.Helpers;

namespace KeremProject1backend.Operations;

public class InstitutionOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly CacheService _cacheService;
    private readonly AuditService _auditService;
    private readonly AuthorizationService _authorizationService;

    public InstitutionOperations(
        ApplicationContext context,
        SessionService sessionService,
        CacheService cacheService,
        AuditService auditService,
        AuthorizationService authorizationService)
    {
        _context = context;
        _sessionService = sessionService;
        _cacheService = cacheService;
        _auditService = auditService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Checks if user is an owner or manager of the institution
    /// </summary>
    private async Task<bool> IsOwnerOrManagerAsync(int institutionId, int userId)
    {
        // Check if user is an owner
        var isOwner = await _context.InstitutionOwners.AnyAsync(
            o => o.InstitutionId == institutionId && o.UserId == userId);
        if (isOwner) return true;

        // Check if user is a manager in InstitutionUsers
        var isManager = await _context.InstitutionUsers.AnyAsync(
            iu => iu.InstitutionId == institutionId &&
                  iu.UserId == userId &&
                  iu.Role == InstitutionRole.Manager &&
                  iu.IsActive);
        return isManager;
    }

    public async Task<BaseResponse<bool>> AddUserToInstitutionAsync(int institutionId, int userId, InstitutionRole role, string? number = null)
    {
        // 1. YETKİ KONTROLÜ
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.Manager,
            UserRole.AdminAdmin,
            UserRole.Admin);
        if (authError != null)
            return BaseResponse<bool>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

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

        // Invalidate the target user's permission cache
        await _sessionService.InvalidateUserCacheAsync(userId);

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
        // 1. Get where user is a member (Manager, Teacher, Student)
        var memberInstitutions = await _context.InstitutionUsers
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

        // 2. Get where user is an Owner
        var ownedInstitutions = await _context.InstitutionOwners
            .Include(io => io.Institution)
            .Where(io => io.UserId == userId)
            .Select(io => new MyInstitutionDto
            {
                Id = io.InstitutionId,
                Name = io.Institution.Name,
                Role = "Owner",
                Status = io.Institution.Status.ToString(),
                JoinedAt = io.AddedAt
            })
            .ToListAsync();

        // 3. Merge (Avoid duplicates if user is both owner and member)
        var allInstitutions = memberInstitutions
            .Concat(ownedInstitutions)
            .GroupBy(i => i.Id)
            .Select(g => g.First()) // Prefer Member role or Owner role? usually just showing once is enough.
                                    // Or maybe show "Owner" if both exist?
                                    // Let's prefer Owner if exists.
            .Select(i =>
            {
                var isOwner = ownedInstitutions.Any(o => o.Id == i.Id);
                if (isOwner) i.Role = "Owner";
                return i;
            })
            .ToList();

        return BaseResponse<List<MyInstitutionDto>>.SuccessResponse(allInstitutions);
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
            .Include(i => i.Owners)
            .ThenInclude(o => o.User)
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
            ManagerName = institution.Owners.FirstOrDefault(o => o.IsPrimaryOwner)?.User.FullName ?? "Unknown",
            ManagerEmail = institution.Owners.FirstOrDefault(o => o.IsPrimaryOwner)?.User.Email ?? "Unknown",
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
        // Authorization: Only owner or manager can update
        var hasAccess = await IsOwnerOrManagerAsync(institutionId, userId);

        if (!hasAccess && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
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
        // Authorization: Only owner or manager can add members
        var hasAccess = await IsOwnerOrManagerAsync(institutionId, userId);

        if (!hasAccess && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
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
        // Authorization: Only owner or manager can remove members
        var hasAccess = await IsOwnerOrManagerAsync(institutionId, userId);

        if (!hasAccess && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
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
        // Authorization: Only owner or manager can update roles
        var hasAccess = await IsOwnerOrManagerAsync(institutionId, userId);

        if (!hasAccess && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
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
    /// <summary>
    /// Checks if user is an owner of the institution
    /// </summary>
    private async Task<bool> IsOwnerAsync(int institutionId, int userId)
    {
        return await _context.InstitutionOwners.AnyAsync(
            o => o.InstitutionId == institutionId && o.UserId == userId);
    }

    public async Task<BaseResponse<int>> CreateManagerAsync(int institutionId, CreateManagerRequest request, int addedByUserId)
    {
        // Authorization: Only owner or AdminAdmin can create manager
        var isOwner = await IsOwnerAsync(institutionId, addedByUserId);
        if (!isOwner && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<int>.ErrorResponse("Access denied. Only owners can create managers.", ErrorCodes.AccessDenied);

        // Check if email exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BaseResponse<int>.ErrorResponse("Email already registered", "USER_EXISTS");

        // Hash password
        KeremProject1backend.Core.Helpers.PasswordHelper.CreateHash(request.Password, out byte[] hash, out byte[] salt);

        // 1. Create User
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Username = request.Email, // Default username = email
            PasswordHash = hash,
            PasswordSalt = salt,
            Phone = request.Phone,
            GlobalRole = UserRole.Manager, // Global role
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 2. Add to Institution as Manager
        var institutionUser = new InstitutionUser
        {
            UserId = user.Id,
            InstitutionId = institutionId,
            Role = InstitutionRole.Manager,
            IsActive = true,
            JoinedAt = DateTime.UtcNow,
            EmployeeNumber = "MNG-" + new Random().Next(1000, 9999) // Auto-generate simple number
        };

        _context.InstitutionUsers.Add(institutionUser);
        await _context.SaveChangesAsync();

        // Audit & Log
        await _auditService.LogAsync(addedByUserId, "ManagerCreated",
            System.Text.Json.JsonSerializer.Serialize(new { InstitutionId = institutionId, NewManagerId = user.Id }));

        return BaseResponse<int>.SuccessResponse(user.Id);
    }

    public async Task<BaseResponse<string>> UpdateManagerAsync(int institutionId, int managerId, UpdateUserRequest request, int currentUserId)
    {
        // Authorization: Only owner or AdminAdmin
        var isOwner = await IsOwnerAsync(institutionId, currentUserId);
        if (!isOwner && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<string>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        var manager = await _context.InstitutionUsers
            .Include(iu => iu.User)
            .FirstOrDefaultAsync(iu => iu.InstitutionId == institutionId && iu.UserId == managerId && iu.Role == InstitutionRole.Manager && iu.IsActive);

        if (manager == null)
            return BaseResponse<string>.ErrorResponse("Manager not found", ErrorCodes.GenericError);

        // Update User info
        if (!string.IsNullOrWhiteSpace(request.FullName)) manager.User.FullName = request.FullName;
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            // Check duplications if email changed
            if (manager.User.Email != request.Email && await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BaseResponse<string>.ErrorResponse("Email already taken", "USER_EXISTS");

            manager.User.Email = request.Email;
            manager.User.Username = request.Email;
        }
        if (!string.IsNullOrWhiteSpace(request.Phone)) manager.User.Phone = request.Phone;

        await _context.SaveChangesAsync();

        // Invalidate cache
        await _sessionService.InvalidateUserCacheAsync(managerId);
        await _cacheService.RemoveByPatternAsync($"institution_detail_{institutionId}");

        return BaseResponse<string>.SuccessResponse("Manager updated successfully");
    }

    public async Task<BaseResponse<string>> RemoveManagerAsync(int institutionId, int managerId, int currentUserId)
    {
        // Authorization: Only owner or AdminAdmin
        var isOwner = await IsOwnerAsync(institutionId, currentUserId);
        if (!isOwner && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<string>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        var manager = await _context.InstitutionUsers
            .FirstOrDefaultAsync(iu => iu.InstitutionId == institutionId && iu.UserId == managerId && iu.Role == InstitutionRole.Manager && iu.IsActive);

        if (manager == null)
            return BaseResponse<string>.ErrorResponse("Manager not found", ErrorCodes.GenericError);

        // Soft delete (set IsActive = false)
        manager.IsActive = false;

        // Also update GlobalRole if necessary? No, user might still be valid globally. 
        // But for InstitutionContext they are removed.

        await _context.SaveChangesAsync();

        // Invalidate
        await _sessionService.InvalidateUserCacheAsync(managerId);
        await _cacheService.RemoveByPatternAsync($"institution_detail_{institutionId}");

        await _auditService.LogAsync(currentUserId, "ManagerRemoved",
            System.Text.Json.JsonSerializer.Serialize(new { InstitutionId = institutionId, ManagerId = managerId }));

        return BaseResponse<string>.SuccessResponse("Manager removed successfully");
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
