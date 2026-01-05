using KeremProject1backend.Core.Constants;
using KeremProject1backend.Core.Helpers;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Models.DTOs.Responses;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;

namespace KeremProject1backend.Operations;

public class AdminOperations
{
    private readonly ApplicationContext _context;
    private readonly AuditService _auditService;
    private readonly SessionService _sessionService;
    private readonly CacheService _cacheService;

    public AdminOperations(ApplicationContext context, AuditService auditService, SessionService sessionService, CacheService cacheService)
    {
        _context = context;
        _auditService = auditService;
        _sessionService = sessionService;
        _cacheService = cacheService;
    }

    // Kullanıcı Yönetimi
    public async Task<BaseResponse<PagedResult<UserListDto>>> GetAllUsersAsync(
        int page = 1, 
        int limit = 20, 
        string? search = null, 
        UserStatus? status = null, 
        UserRole? role = null,
        bool forceRefresh = false)
    {
        // Cache key: admin users list with filters
        var cacheKey = $"admin_users_{page}_{limit}_{search}_{status}_{role}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResult<UserListDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResult<UserListDto>>.SuccessResponse(cached);
        }

        var query = _context.Users
            .AsNoTracking() // Read-only, optimize
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => 
                u.Username.Contains(search) ||
                u.FullName.Contains(search) ||
                u.Email.Contains(search));
        }

        if (status.HasValue)
            query = query.Where(u => u.Status == status.Value);

        if (role.HasValue)
            query = query.Where(u => u.GlobalRole == role.Value);

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Username = u.Username,
                Email = u.Email,
                GlobalRole = u.GlobalRole.ToString(),
                Status = u.Status.ToString(),
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        var result = new PagedResult<UserListDto>
        {
            Items = users,
            Total = total,
            Page = page,
            Limit = limit,
            TotalPages = (int)Math.Ceiling(total / (double)limit)
        };

        // Cache for 2 minutes (user lists change when new users are added)
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));
        }

        return BaseResponse<PagedResult<UserListDto>>.SuccessResponse(result);
    }

    public async Task<BaseResponse<UserDetailDto>> GetUserAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.InstitutionMemberships)
                .ThenInclude(im => im.Institution)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return BaseResponse<UserDetailDto>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        var detail = new UserDetailDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            GlobalRole = user.GlobalRole.ToString(),
            Status = user.Status.ToString(),
            ProfileImageUrl = user.ProfileImageUrl,
            ProfileVisibility = user.ProfileVisibility.ToString(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Institutions = user.InstitutionMemberships.Select(im => new InstitutionSummaryDto
            {
                Id = im.InstitutionId,
                Name = im.Institution.Name,
                Role = im.Role.ToString()
            }).ToList()
        };

        return BaseResponse<UserDetailDto>.SuccessResponse(detail);
    }

    public async Task<BaseResponse<string>> UpdateUserAsync(int userId, AdminUpdateUserRequest request, int adminId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
                return BaseResponse<string>.ErrorResponse("Email already registered", ErrorCodes.AuthEmailTaken);
            user.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
            user.Phone = request.Phone;

        if (request.GlobalRole.HasValue)
            user.GlobalRole = request.GlobalRole.Value;

        if (request.Status.HasValue)
            user.Status = request.Status.Value;

        await _context.SaveChangesAsync();

        // Invalidate related caches
        await _cacheService.InvalidateUserCacheAsync(userId);
        await _cacheService.InvalidateAdminCacheAsync();
        await _cacheService.RemoveByPatternAsync("admin_users_*"); // Invalidate all user list caches

        await _auditService.LogAsync(adminId, "UserUpdatedByAdmin", 
            JsonSerializer.Serialize(new { UserId = userId, Changes = request }));

        return BaseResponse<string>.SuccessResponse("User updated successfully");
    }

    public async Task<BaseResponse<string>> UpdateUserStatusAsync(int userId, UserStatus status, int adminId, string? reason = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        user.Status = status;
        await _context.SaveChangesAsync();

        // Invalidate caches
        await _cacheService.InvalidateUserCacheAsync(userId);
        await _cacheService.InvalidateAdminCacheAsync();
        await _cacheService.RemoveByPatternAsync("admin_users_*");

        await _auditService.LogAsync(adminId, "UserStatusChanged", 
            JsonSerializer.Serialize(new { UserId = userId, NewStatus = status.ToString(), Reason = reason }));

        return BaseResponse<string>.SuccessResponse($"User status changed to {status}");
    }

    public async Task<BaseResponse<string>> DeleteUserAsync(int userId, int adminId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        // Hard delete
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Invalidate caches
        await _cacheService.InvalidateUserCacheAsync(userId);
        await _cacheService.InvalidateAdminCacheAsync();
        await _cacheService.RemoveByPatternAsync("admin_users_*");

        await _auditService.LogAsync(adminId, "UserDeletedByAdmin", 
            JsonSerializer.Serialize(new { UserId = userId }));

        return BaseResponse<string>.SuccessResponse("User deleted successfully");
    }

    public async Task<BaseResponse<string>> ResetUserPasswordAsync(int userId, int adminId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        // Generate random password
        var newPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12))
            .Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 12);

        PasswordHelper.CreateHash(newPassword, out byte[] hash, out byte[] salt);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        await _context.SaveChangesAsync();

        // TODO: Send email with new password
        await _auditService.LogAsync(adminId, "UserPasswordResetByAdmin", 
            JsonSerializer.Serialize(new { UserId = userId }));

        return BaseResponse<string>.SuccessResponse($"Password reset. New password: {newPassword}"); // In production, send via email
    }

    // Kurum Yönetimi
    public async Task<BaseResponse<PagedResult<InstitutionListDto>>> GetAllInstitutionsAsync(
        int page = 1,
        int limit = 20,
        InstitutionStatus? status = null,
        string? search = null)
    {
        var query = _context.Institutions
            .Include(i => i.Manager)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => 
                i.Name.Contains(search) ||
                i.LicenseNumber.Contains(search));
        }

        var total = await query.CountAsync();
        var institutions = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(i => new InstitutionListDto
            {
                Id = i.Id,
                Name = i.Name,
                LicenseNumber = i.LicenseNumber,
                ManagerName = i.Manager.FullName,
                Status = i.Status.ToString(),
                CreatedAt = i.CreatedAt,
                SubscriptionEndDate = i.SubscriptionEndDate
            })
            .ToListAsync();

        var result = new PagedResult<InstitutionListDto>
        {
            Items = institutions,
            Total = total,
            Page = page,
            Limit = limit,
            TotalPages = (int)Math.Ceiling(total / (double)limit)
        };

        return BaseResponse<PagedResult<InstitutionListDto>>.SuccessResponse(result);
    }

    public async Task<BaseResponse<InstitutionDetailDto>> GetInstitutionAsync(int institutionId)
    {
        var institution = await _context.Institutions
            .Include(i => i.Manager)
            .FirstOrDefaultAsync(i => i.Id == institutionId);

        if (institution == null)
            return BaseResponse<InstitutionDetailDto>.ErrorResponse("Institution not found", ErrorCodes.AdminInstitutionNotFound);

        var memberCount = await _context.InstitutionUsers.CountAsync(iu => iu.InstitutionId == institutionId);
        var studentCount = await _context.InstitutionUsers.CountAsync(iu => 
            iu.InstitutionId == institutionId && iu.Role == InstitutionRole.Student);
        var teacherCount = await _context.InstitutionUsers.CountAsync(iu => 
            iu.InstitutionId == institutionId && iu.Role == InstitutionRole.Teacher);
        var classroomCount = await _context.Classrooms.CountAsync(c => c.InstitutionId == institutionId);
        var examCount = await _context.Exams.CountAsync(e => e.InstitutionId == institutionId);

        var detail = new InstitutionDetailDto
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

        return BaseResponse<InstitutionDetailDto>.SuccessResponse(detail);
    }

    public async Task<BaseResponse<string>> RejectInstitutionAsync(int institutionId, string reason, int adminId)
    {
        var institution = await _context.Institutions
            .Include(i => i.Manager)
            .FirstOrDefaultAsync(i => i.Id == institutionId);

        if (institution == null)
            return BaseResponse<string>.ErrorResponse("Institution not found", ErrorCodes.AdminInstitutionNotFound);

        if (institution.Status != InstitutionStatus.PendingApproval)
            return BaseResponse<string>.ErrorResponse("Institution is not pending approval", ErrorCodes.AdminInstitutionNotPending);

        institution.Status = InstitutionStatus.Suspended; // Or create Rejected status
        await _context.SaveChangesAsync();

        // TODO: Send notification to manager
        await _auditService.LogAsync(adminId, "InstitutionRejected", 
            JsonSerializer.Serialize(new { InstitutionId = institutionId, Reason = reason }));

        return BaseResponse<string>.SuccessResponse("Institution rejected");
    }

    public async Task<BaseResponse<string>> UpdateInstitutionStatusAsync(int institutionId, InstitutionStatus status, int adminId, string? reason = null)
    {
        var institution = await _context.Institutions.FindAsync(institutionId);
        if (institution == null)
            return BaseResponse<string>.ErrorResponse("Institution not found", ErrorCodes.AdminInstitutionNotFound);

        institution.Status = status;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(adminId, "InstitutionStatusChanged", 
            JsonSerializer.Serialize(new { InstitutionId = institutionId, NewStatus = status.ToString(), Reason = reason }));

        return BaseResponse<string>.SuccessResponse($"Institution status changed to {status}");
    }

    public async Task<BaseResponse<string>> ExtendSubscriptionAsync(int institutionId, int months, int adminId)
    {
        var institution = await _context.Institutions.FindAsync(institutionId);
        if (institution == null)
            return BaseResponse<string>.ErrorResponse("Institution not found", ErrorCodes.AdminInstitutionNotFound);

        if (institution.SubscriptionEndDate.HasValue)
        {
            institution.SubscriptionEndDate = institution.SubscriptionEndDate.Value.AddMonths(months);
        }
        else
        {
            institution.SubscriptionStartDate = DateTime.UtcNow;
            institution.SubscriptionEndDate = DateTime.UtcNow.AddMonths(months);
        }

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(adminId, "InstitutionSubscriptionExtended", 
            JsonSerializer.Serialize(new { InstitutionId = institutionId, Months = months }));

        return BaseResponse<string>.SuccessResponse($"Subscription extended by {months} months");
    }

    // Admin Hesap Yönetimi
    public async Task<BaseResponse<int>> CreateAdminAsync(CreateAdminRequest request, int adminId)
    {
        // Check if username/email exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BaseResponse<int>.ErrorResponse("Username already taken", ErrorCodes.AuthUsernameTaken);

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BaseResponse<int>.ErrorResponse("Email already registered", ErrorCodes.AuthEmailTaken);

        PasswordHelper.CreateHash(request.Password, out byte[] hash, out byte[] salt);

        var user = new User
        {
            FullName = request.FullName,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            GlobalRole = request.Role,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(adminId, "AdminCreated", 
            JsonSerializer.Serialize(new { AdminId = user.Id, Role = request.Role.ToString() }));

        return BaseResponse<int>.SuccessResponse(user.Id);
    }

    public async Task<BaseResponse<List<AdminListDto>>> GetAdminsAsync()
    {
        var admins = await _context.Users
            .Where(u => u.GlobalRole == UserRole.Admin || u.GlobalRole == UserRole.AdminAdmin)
            .Select(u => new AdminListDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Username = u.Username,
                Email = u.Email,
                GlobalRole = u.GlobalRole.ToString(),
                Status = u.Status.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return BaseResponse<List<AdminListDto>>.SuccessResponse(admins);
    }

    // İstatistikler
    public async Task<BaseResponse<AdminStatisticsDto>> GetStatisticsAsync(bool forceRefresh = false)
    {
        // Cache key: admin statistics (expensive calculation, frequently accessed)
        var cacheKey = "admin_statistics";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<AdminStatisticsDto>(cacheKey);
            if (cached != null)
                return BaseResponse<AdminStatisticsDto>.SuccessResponse(cached);
        }

        // Optimize: Use single query with grouping instead of multiple queries
        var userStats = await _context.Users
            .AsNoTracking()
            .GroupBy(u => u.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var institutionStats = await _context.Institutions
            .AsNoTracking()
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var totalUsers = userStats.Sum(s => s.Count);
        var activeUsers = userStats.FirstOrDefault(s => s.Status == UserStatus.Active)?.Count ?? 0;
        var suspendedUsers = userStats.FirstOrDefault(s => s.Status == UserStatus.Suspended)?.Count ?? 0;
        var deletedUsers = userStats.FirstOrDefault(s => s.Status == UserStatus.Deleted)?.Count ?? 0;

        var totalInstitutions = institutionStats.Sum(s => s.Count);
        var activeInstitutions = institutionStats.FirstOrDefault(s => s.Status == InstitutionStatus.Active)?.Count ?? 0;
        var pendingInstitutions = institutionStats.FirstOrDefault(s => s.Status == InstitutionStatus.PendingApproval)?.Count ?? 0;
        var suspendedInstitutions = institutionStats.FirstOrDefault(s => s.Status == InstitutionStatus.Suspended)?.Count ?? 0;

        var totalExams = await _context.Exams.AsNoTracking().CountAsync();
        var totalMessages = await _context.Messages.AsNoTracking().CountAsync();

        var last30Days = DateTime.UtcNow.AddDays(-30);
        var newUsersLast30Days = await _context.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= last30Days);
        var activeUsersLast30Days = await _context.Users.AsNoTracking().CountAsync(u => u.LastLoginAt >= last30Days);

        var statistics = new AdminStatisticsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            SuspendedUsers = suspendedUsers,
            DeletedUsers = deletedUsers,
            TotalInstitutions = totalInstitutions,
            ActiveInstitutions = activeInstitutions,
            PendingInstitutions = pendingInstitutions,
            SuspendedInstitutions = suspendedInstitutions,
            TotalExams = totalExams,
            TotalMessages = totalMessages,
            NewUsersLast30Days = newUsersLast30Days,
            ActiveUsersLast30Days = activeUsersLast30Days
        };

        // Cache for 5 minutes (statistics update when data changes)
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, statistics, TimeSpan.FromMinutes(5));
        }

        return BaseResponse<AdminStatisticsDto>.SuccessResponse(statistics);
    }

    // Audit Logs
    public async Task<BaseResponse<PagedResult<AuditLogDto>>> GetAuditLogsAsync(
        int? userId = null,
        string? action = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 50)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (userId.HasValue)
            query = query.Where(al => al.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(al => al.Action.Contains(action));

        if (dateFrom.HasValue)
            query = query.Where(al => al.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(al => al.CreatedAt <= dateTo.Value);

        var total = await query.CountAsync();
        var logs = await query
            .OrderByDescending(al => al.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        // Get user names separately
        var userIds = logs.Where(al => al.UserId.HasValue).Select(al => al.UserId!.Value).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName);

        var logDtos = logs.Select(al => new AuditLogDto
        {
            Id = al.Id,
            UserId = al.UserId,
            UserName = al.UserId.HasValue && users.ContainsKey(al.UserId.Value) 
                ? users[al.UserId.Value] 
                : "System",
            Action = al.Action,
            Details = al.Details,
            IpAddress = al.IpAddress,
            CreatedAt = al.CreatedAt
        }).ToList();

        var result = new PagedResult<AuditLogDto>
        {
            Items = logDtos,
            Total = total,
            Page = page,
            Limit = limit,
            TotalPages = (int)Math.Ceiling(total / (double)limit)
        };

        return BaseResponse<PagedResult<AuditLogDto>>.SuccessResponse(result);
    }
}

// DTOs
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
}

public class UserListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GlobalRole { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UserDetailDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string GlobalRole { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string ProfileVisibility { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<InstitutionSummaryDto> Institutions { get; set; } = new();
}

public class InstitutionListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}

public class InstitutionDetailDto
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

public class AdminListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GlobalRole { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminStatisticsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int SuspendedUsers { get; set; }
    public int DeletedUsers { get; set; }
    public int TotalInstitutions { get; set; }
    public int ActiveInstitutions { get; set; }
    public int PendingInstitutions { get; set; }
    public int SuspendedInstitutions { get; set; }
    public int TotalExams { get; set; }
    public int TotalMessages { get; set; }
    public int NewUsersLast30Days { get; set; }
    public int ActiveUsersLast30Days { get; set; }
}

public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public UserRole? GlobalRole { get; set; }
    public UserStatus? Status { get; set; }
}

public class CreateAdminRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Admin;
}

