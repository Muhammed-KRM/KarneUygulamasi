using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Operations;

public class InstitutionOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;

    public InstitutionOperations(ApplicationContext context, SessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
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
