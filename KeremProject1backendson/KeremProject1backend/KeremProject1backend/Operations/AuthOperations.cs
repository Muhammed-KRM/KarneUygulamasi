using KeremProject1backend.Core.Helpers;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Models.DTOs.Responses;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using KeremProject1backend.Core.Constants; // Added this
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Operations;

public static class AuthOperations
{
    public static async Task<BaseResponse<string>> RegisterAsync(
        RegisterRequest request,
        ApplicationContext context,
        AuditService auditService)
    {
        // 1. Username/Email uniqueness check
        if (await context.Users.AnyAsync(u => u.Username == request.Username))
            return BaseResponse<string>.ErrorResponse("Username already taken", ErrorCodes.AuthUsernameTaken);

        if (await context.Users.AnyAsync(u => u.Email == request.Email))
            return BaseResponse<string>.ErrorResponse("Email already registered", ErrorCodes.AuthEmailTaken);

        // 2. Password hash
        PasswordHelper.CreateHash(request.Password, out byte[] hash, out byte[] salt);

        // 3. Create User
        var user = new User
        {
            FullName = request.FullName,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            GlobalRole = UserRole.User,
            Status = UserStatus.Active,
            ProfileVisibility = ProfileVisibility.PublicToAll,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // 4. Audit log
        await auditService.LogAsync(user.Id, "UserRegistered", $"Username: {user.Username}");

        return BaseResponse<string>.SuccessResponse("Registration successful. You can now log in.");
    }

    public static async Task<BaseResponse<LoginResponse>> LoginAsync(
        LoginRequest request,
        ApplicationContext context,
        SessionService sessionService,
        AuditService auditService)
    {
        // 1. Find user
        var user = await context.Users
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
        var token = sessionService.GenerateToken(user, user.InstitutionMemberships.ToList());

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

        context.RefreshTokens.Add(refreshTokenEntity);

        // 6. Update LastLogin
        user.LastLoginAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // 7. Audit log
        await auditService.LogAsync(user.Id, "UserLoggedIn", null);

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

    public static async Task<BaseResponse<string>> ApplyInstitutionAsync(
        ApplyInstitutionRequest request,
        int userId,
        ApplicationContext context,
        AuditService auditService)
    {
        // 1. Check if user already has a pending or active institution application?
        // (Optional rule: One institution per user for now?)

        // 2. Uniqueness check for License Number
        if (await context.Institutions.AnyAsync(i => i.LicenseNumber == request.LicenseNumber))
            return BaseResponse<string>.ErrorResponse("License number already registered", ErrorCodes.AuthLicenseNumberTaken);

        // 3. Create Institution
        var institution = new Institution
        {
            Name = request.Name,
            LicenseNumber = request.LicenseNumber,
            Address = request.Address,
            Phone = request.Phone,
            ManagerUserId = userId,
            Status = InstitutionStatus.PendingApproval, // Pending admin approval
            CreatedAt = DateTime.UtcNow
        };

        context.Institutions.Add(institution);
        await context.SaveChangesAsync();

        // 4. Create InstitutionUser (Manager role)
        var membership = new InstitutionUser
        {
            UserId = userId,
            InstitutionId = institution.Id,
            Role = InstitutionRole.Manager,
            JoinedAt = DateTime.UtcNow
        };

        context.InstitutionUsers.Add(membership);
        await context.SaveChangesAsync();

        // 5. Audit log
        await auditService.LogAsync(userId, "InstitutionApplied", $"Institution: {institution.Name}");

        return BaseResponse<string>.SuccessResponse("Application submitted. Waiting for admin approval.");
    }

    public static async Task<BaseResponse<string>> ApproveInstitutionAsync(
        int institutionId,
        int adminId,
        ApplicationContext context,
        AuditService auditService)
    {
        var institution = await context.Institutions.FindAsync(institutionId);
        if (institution == null)
            return BaseResponse<string>.ErrorResponse("Institution not found", ErrorCodes.AdminInstitutionNotFound);

        if (institution.Status != InstitutionStatus.PendingApproval)
            return BaseResponse<string>.ErrorResponse("Institution is not pending approval", ErrorCodes.AdminInstitutionNotPending);

        institution.Status = InstitutionStatus.Active;
        institution.ApprovedAt = DateTime.UtcNow;
        institution.ApprovedByAdminId = adminId;
        institution.SubscriptionStartDate = DateTime.UtcNow;
        institution.SubscriptionEndDate = DateTime.UtcNow.AddYears(1); // 1 year trial/sub

        await context.SaveChangesAsync();

        await auditService.LogAsync(adminId, "InstitutionApproved", $"InstitutionId: {institutionId}");

        return BaseResponse<string>.SuccessResponse("Institution approved successfully.");
    }

    public static async Task<BaseResponse<List<Institution>>> GetPendingInstitutionsAsync(ApplicationContext context)
    {
        var pending = await context.Institutions
            .Where(i => i.Status == InstitutionStatus.PendingApproval)
            .Include(i => i.Manager)
            .ToListAsync();

        return BaseResponse<List<Institution>>.SuccessResponse(pending);
    }

    public static async Task<BaseResponse<LoginResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        ApplicationContext context,
        SessionService sessionService,
        AuditService auditService)
    {
        var refreshToken = await context.RefreshTokens
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
        var newToken = sessionService.GenerateToken(user, user.InstitutionMemberships.ToList());

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

        context.RefreshTokens.Add(newRefreshTokenEntity);
        await context.SaveChangesAsync();

        await auditService.LogAsync(user.Id, "TokenRefreshed", null);

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
