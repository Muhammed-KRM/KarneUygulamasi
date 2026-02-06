using KeremProject1backend.Core.Helpers;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Models.DTOs.Responses;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using KeremProject1backend.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Operations;

public class AuthOperations
{
    private readonly ApplicationContext _context;
    private readonly AuditService _auditService;
    private readonly SessionService _sessionService;

    public AuthOperations(
        ApplicationContext context,
        AuditService auditService,
        SessionService sessionService)
    {
        _context = context;
        _auditService = auditService;
        _sessionService = sessionService;
    }

    public async Task<BaseResponse<string>> RegisterAsync(RegisterRequest request)
    {
        // 1. Username/Email uniqueness check
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BaseResponse<string>.ErrorResponse("Username already taken", ErrorCodes.AuthUsernameTaken);

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BaseResponse<string>.ErrorResponse("Email already registered", ErrorCodes.AuthEmailTaken);

        // 2. Password hash
        PasswordHelper.CreateHash(request.Password, out byte[] hash, out byte[] salt);

        // 3. Determine role based on registration type
        UserRole targetRole = UserRole.Student;
        if (request.RegisterAsOwner)
        {
            targetRole = UserRole.InstitutionOwner;
        }

        // 4. Create User
        var user = new User
        {
            FullName = request.FullName,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            GlobalRole = targetRole,
            Status = UserStatus.Active,
            ProfileVisibility = ProfileVisibility.PublicToAll,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 5. Audit log
        await _auditService.LogAsync(user.Id, "UserRegistered", $"Username: {user.Username}, Role: {targetRole}");

        return BaseResponse<string>.SuccessResponse("Registration successful. You can now log in.");
    }

    public async Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        // 1. Find user
        var user = await _context.Users
            .Include(u => u.InstitutionMemberships)
                .ThenInclude(im => im.Institution)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            return BaseResponse<LoginResponse>.ErrorResponse("Invalid username or password", ErrorCodes.AuthInvalidCredentials);

        // 2. Verify password
        if (!PasswordHelper.VerifyHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return BaseResponse<LoginResponse>.ErrorResponse("Invalid username or password", ErrorCodes.AuthInvalidCredentials);

        // 3. Status check
        if (user.Status == UserStatus.Suspended)
            return BaseResponse<LoginResponse>.ErrorResponse("Your account has been suspended", ErrorCodes.AuthUserSuspended);

        if (user.Status == UserStatus.Deleted)
            return BaseResponse<LoginResponse>.ErrorResponse("Account not found", ErrorCodes.AuthUserDeleted);

        // 4. Generate token
        var token = _sessionService.GenerateToken(user, user.InstitutionMemberships.ToList());

        // 5. Generate refresh token
        var refreshToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // 30 days for refresh token
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        // 6. Update LastLogin
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // 6. Audit log
        await _auditService.LogAsync(user.Id, "UserLoggedIn", null);

        // 8. Build response
        var response = new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                GlobalRole = user.GlobalRole.ToString(),
                Institutions = user.InstitutionMemberships.Select(im => new InstitutionSummaryDto
                {
                    Id = im.InstitutionId,
                    Name = im.Institution.Name,
                    Role = im.Role.ToString()
                }).ToList()
            }
        };

        return BaseResponse<LoginResponse>.SuccessResponse(response);
    }

    public async Task<BaseResponse<string>> ApplyInstitutionAsync(ApplyInstitutionRequest request, int userId)
    {
        // 1. Check if user is InstitutionOwner
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.NotFound);

        if (user.GlobalRole != UserRole.InstitutionOwner &&
            user.GlobalRole != UserRole.AdminAdmin &&
            user.GlobalRole != UserRole.Admin)
            return BaseResponse<string>.ErrorResponse("Only Institution Owners can apply for institutions", ErrorCodes.AccessDenied);

        // 2. Uniqueness check for License Number
        if (await _context.Institutions.AnyAsync(i => i.LicenseNumber == request.LicenseNumber))
            return BaseResponse<string>.ErrorResponse("License number already registered", ErrorCodes.AuthLicenseNumberTaken);

        // 3. Create Institution
        var institution = new Institution
        {
            Name = request.Name,
            LicenseNumber = request.LicenseNumber,
            Address = request.Address,
            Phone = request.Phone,
            Status = InstitutionStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow
        };

        _context.Institutions.Add(institution);
        await _context.SaveChangesAsync();

        // 4. Create InstitutionOwner (Primary owner)
        var ownership = new InstitutionOwner
        {
            UserId = userId,
            InstitutionId = institution.Id,
            IsPrimaryOwner = true,
            AddedAt = DateTime.UtcNow,
            AddedByUserId = null  // Self-applied
        };

        _context.InstitutionOwners.Add(ownership);
        await _context.SaveChangesAsync();

        // 5. Audit log
        await _auditService.LogAsync(userId, "InstitutionApplied", $"Institution: {institution.Name}");

        return BaseResponse<string>.SuccessResponse("Application submitted. Waiting for admin approval.");
    }

    public async Task<BaseResponse<string>> ApproveInstitutionAsync(int institutionId, int adminId)
    {
        var institution = await _context.Institutions
            .Include(i => i.Owners)
            .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(i => i.Id == institutionId);

        if (institution == null)
            return BaseResponse<string>.ErrorResponse("Institution not found", ErrorCodes.AdminInstitutionNotFound);

        if (institution.Status != InstitutionStatus.PendingApproval)
            return BaseResponse<string>.ErrorResponse("Institution is not pending approval", ErrorCodes.AdminInstitutionNotPending);

        institution.Status = InstitutionStatus.Active;
        institution.ApprovedAt = DateTime.UtcNow;
        institution.ApprovedByAdminId = adminId;
        institution.SubscriptionStartDate = DateTime.UtcNow;
        institution.SubscriptionEndDate = DateTime.UtcNow.AddYears(1);

        await _context.SaveChangesAsync();

        // Invalidate all owners' permission cache
        foreach (var owner in institution.Owners)
        {
            await _sessionService.InvalidateUserCacheAsync(owner.UserId);
        }

        await _auditService.LogAsync(adminId, "InstitutionApproved", $"InstitutionId: {institutionId}");

        return BaseResponse<string>.SuccessResponse("Institution approved successfully.");
    }

    public async Task<BaseResponse<List<Institution>>> GetPendingInstitutionsAsync()
    {
        var pending = await _context.Institutions
            .Where(i => i.Status == InstitutionStatus.PendingApproval)
            .Include(i => i.Owners)
            .ThenInclude(o => o.User)
            .ToListAsync();

        return BaseResponse<List<Institution>>.SuccessResponse(pending);
    }

    public async Task<BaseResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.InstitutionMemberships)
                    .ThenInclude(im => im.Institution)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked);

        if (refreshToken == null)
            return BaseResponse<LoginResponse>.ErrorResponse("Invalid refresh token", ErrorCodes.AuthInvalidCredentials);

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
            return BaseResponse<LoginResponse>.ErrorResponse("Refresh token has expired", ErrorCodes.AuthInvalidCredentials);

        var user = refreshToken.User;

        // Revoke old refresh token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;

        // Generate new access token
        var newToken = _sessionService.GenerateToken(user, user.InstitutionMemberships.ToList());

        // Generate new refresh token
        var newRefreshToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(user.Id, "TokenRefreshed", null);

        var response = new LoginResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RefreshTokenExpiresAt = newRefreshTokenEntity.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                GlobalRole = user.GlobalRole.ToString(),
                Institutions = user.InstitutionMemberships.Select(im => new InstitutionSummaryDto
                {
                    Id = im.InstitutionId,
                    Name = im.Institution.Name,
                    Role = im.Role.ToString()
                }).ToList()
            }
        };

        return BaseResponse<LoginResponse>.SuccessResponse(response);
    }
}
