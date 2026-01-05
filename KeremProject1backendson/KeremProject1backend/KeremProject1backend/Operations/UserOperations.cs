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
using System.Text;
using System.Text.Json;
using LessonScore = KeremProject1backend.Services.LessonScore;

namespace KeremProject1backend.Operations;

public class UserOperations
{
    private readonly ApplicationContext _context;
    private readonly AuditService _auditService;
    private readonly CacheService _cacheService;

    public UserOperations(ApplicationContext context, AuditService auditService, CacheService cacheService)
    {
        _context = context;
        _auditService = auditService;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<UserProfileDto>> GetProfileAsync(int userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return BaseResponse<UserProfileDto>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        var profile = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            ProfileImageUrl = user.ProfileImageUrl,
            ProfileVisibility = user.ProfileVisibility.ToString(),
            GlobalRole = user.GlobalRole.ToString(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return BaseResponse<UserProfileDto>.SuccessResponse(profile);
    }

    public async Task<BaseResponse<string>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrWhiteSpace(request.Phone))
            user.Phone = request.Phone;

        if (request.ProfileVisibility.HasValue)
            user.ProfileVisibility = request.ProfileVisibility.Value;

        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"User:{userId}:Profile");

        await _auditService.LogAsync(userId, "ProfileUpdated", $"Updated: {string.Join(", ", new[] { request.FullName, request.Phone }.Where(x => !string.IsNullOrEmpty(x)))}");

        return BaseResponse<string>.SuccessResponse("Profile updated successfully");
    }

    public async Task<BaseResponse<string>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        // Verify current password
        if (!PasswordHelper.VerifyHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            return BaseResponse<string>.ErrorResponse("Current password is incorrect", ErrorCodes.AuthInvalidCredentials);

        // Validate new password
        if (request.NewPassword.Length < 8)
            return BaseResponse<string>.ErrorResponse("New password must be at least 8 characters", ErrorCodes.ValidationFailed);

        // Update password
        PasswordHelper.CreateHash(request.NewPassword, out byte[] hash, out byte[] salt);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(userId, "PasswordChanged", null);

        return BaseResponse<string>.SuccessResponse("Password changed successfully");
    }

    public async Task<BaseResponse<string>> UploadProfileImageAsync(int userId, IFormFile file)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        // Validate file
        if (file == null || file.Length == 0)
            return BaseResponse<string>.ErrorResponse("File is required", ErrorCodes.ValidationFailed);

        if (file.Length > 5 * 1024 * 1024) // 5MB
            return BaseResponse<string>.ErrorResponse("File size must be less than 5MB", ErrorCodes.ValidationFailed);

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BaseResponse<string>.ErrorResponse("Invalid file type. Allowed: jpg, jpeg, png, gif, webp", ErrorCodes.ValidationFailed);

        // Delete old image if exists
        if (!string.IsNullOrEmpty(user.ProfileImageUrl))
        {
            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Users", Path.GetFileName(user.ProfileImageUrl));
            if (File.Exists(oldImagePath))
            {
                try { File.Delete(oldImagePath); } catch { }
            }
        }

        // Save new image
        var fileName = $"{userId:D7}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Users");
        Directory.CreateDirectory(uploadPath);
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        user.ProfileImageUrl = $"/Files/Users/{fileName}";
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"User:{userId}:Profile");

        await _auditService.LogAsync(userId, "ProfileImageUploaded", $"File: {fileName}");

        return BaseResponse<string>.SuccessResponse(user.ProfileImageUrl);
    }

    public async Task<BaseResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            // Don't reveal if email exists for security
            return BaseResponse<string>.SuccessResponse("If the email exists, a password reset link has been sent.");
        }

        // Generate reset token
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace("+", "-").Replace("/", "_").Replace("=", "");
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24), // 24 hours validity
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        // TODO: Send email with reset link
        // For now, we'll just log it (in production, use email service)
        await _auditService.LogAsync(user.Id, "PasswordResetRequested", $"Token: {token}");

        return BaseResponse<string>.SuccessResponse("If the email exists, a password reset link has been sent.");
    }

    public async Task<BaseResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var resetToken = await _context.PasswordResetTokens
            .Include(prt => prt.User)
            .FirstOrDefaultAsync(prt => prt.Token == request.Token && !prt.IsUsed);

        if (resetToken == null)
            return BaseResponse<string>.ErrorResponse("Invalid or expired reset token", ErrorCodes.ValidationFailed);

        if (resetToken.ExpiresAt < DateTime.UtcNow)
            return BaseResponse<string>.ErrorResponse("Reset token has expired", ErrorCodes.ValidationFailed);

        // Validate new password
        if (request.NewPassword.Length < 8)
            return BaseResponse<string>.ErrorResponse("Password must be at least 8 characters", ErrorCodes.ValidationFailed);

        // Update password
        PasswordHelper.CreateHash(request.NewPassword, out byte[] hash, out byte[] salt);
        resetToken.User.PasswordHash = hash;
        resetToken.User.PasswordSalt = salt;

        // Mark token as used
        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(resetToken.UserId, "PasswordResetCompleted", null);

        return BaseResponse<string>.SuccessResponse("Password has been reset successfully");
    }

    public async Task<BaseResponse<string>> LogoutAsync(int userId, string token)
    {
        // Add token to blacklist in Redis (expires in 7 days, same as token expiry)
        await _cacheService.SetAsync($"Blacklist:Token:{token}", true, TimeSpan.FromDays(7));

        await _auditService.LogAsync(userId, "UserLoggedOut", null);

        return BaseResponse<string>.SuccessResponse("Logged out successfully");
    }

    public async Task<BaseResponse<string>> SendVerificationEmailAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        // Check if already verified
        var existingVerification = await _context.EmailVerifications
            .FirstOrDefaultAsync(ev => ev.UserId == userId && ev.Email == user.Email && ev.IsVerified);
        
        if (existingVerification != null)
            return BaseResponse<string>.ErrorResponse("Email is already verified", ErrorCodes.ValidationFailed);

        // Generate verification token
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");

        var verification = new EmailVerification
        {
            UserId = userId,
            Email = user.Email,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days validity
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailVerifications.Add(verification);
        await _context.SaveChangesAsync();

        // TODO: Send email with verification link
        // For now, we'll just log it (in production, use email service)
        await _auditService.LogAsync(userId, "VerificationEmailSent", $"Token: {token}");

        return BaseResponse<string>.SuccessResponse("Verification email sent. Please check your inbox.");
    }

    public async Task<BaseResponse<string>> VerifyEmailAsync(string token)
    {
        var verification = await _context.EmailVerifications
            .Include(ev => ev.User)
            .FirstOrDefaultAsync(ev => ev.Token == token && !ev.IsVerified);

        if (verification == null)
            return BaseResponse<string>.ErrorResponse("Invalid or expired verification token", ErrorCodes.ValidationFailed);

        if (verification.ExpiresAt < DateTime.UtcNow)
            return BaseResponse<string>.ErrorResponse("Verification token has expired", ErrorCodes.ValidationFailed);

        // Mark as verified
        verification.IsVerified = true;
        verification.VerifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(verification.UserId, "EmailVerified", $"Email: {verification.Email}");

        return BaseResponse<string>.SuccessResponse("Email verified successfully");
    }

    public async Task<BaseResponse<UserProfileDto>> GetUserProfileAsync(int targetUserId, int currentUserId, bool forceRefresh = false)
    {
        // Cache key: user profile (public data)
        var cacheKey = $"user_profile_{targetUserId}_{currentUserId}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<UserProfileDto>(cacheKey);
            if (cached != null)
                return BaseResponse<UserProfileDto>.SuccessResponse(cached);
        }

        var targetUser = await _context.Users
            .Include(u => u.InstitutionMemberships)
                .ThenInclude(im => im.Institution)
            .AsNoTracking() // Read-only, optimize
            .FirstOrDefaultAsync(u => u.Id == targetUserId);

        if (targetUser == null)
            return BaseResponse<UserProfileDto>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        // ProfileVisibility kontrolü
        var isOwner = targetUserId == currentUserId;
        var isTeacher = await _context.InstitutionUsers
            .AsNoTracking()
            .AnyAsync(iu =>
                iu.UserId == currentUserId &&
                iu.Role == InstitutionRole.Teacher);

        var canView = isOwner || 
            targetUser.ProfileVisibility == ProfileVisibility.PublicToAll ||
            (targetUser.ProfileVisibility == ProfileVisibility.TeachersOnly && isTeacher);

        if (!canView)
            return BaseResponse<UserProfileDto>.ErrorResponse("You don't have permission to view this profile", ErrorCodes.AccessDenied);

        var profile = new UserProfileDto
        {
            Id = targetUser.Id,
            FullName = targetUser.FullName,
            Username = targetUser.Username,
            Email = isOwner ? targetUser.Email : null, // Email sadece kendi profili için
            Phone = isOwner ? targetUser.Phone : null, // Phone sadece kendi profili için
            ProfileImageUrl = targetUser.ProfileImageUrl,
            ProfileVisibility = targetUser.ProfileVisibility.ToString(),
            GlobalRole = targetUser.GlobalRole.ToString(),
            CreatedAt = targetUser.CreatedAt,
            LastLoginAt = targetUser.LastLoginAt
        };

        // Cache for 5 minutes (profiles don't change frequently)
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, profile, TimeSpan.FromMinutes(5));
        }

        return BaseResponse<UserProfileDto>.SuccessResponse(profile);
    }

    public async Task<BaseResponse<string>> UpdateEmailAsync(int userId, UpdateEmailRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        // Verify current password
        if (!PasswordHelper.VerifyHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            return BaseResponse<string>.ErrorResponse("Current password is incorrect", ErrorCodes.AuthInvalidCredentials);

        // Check if new email is already taken
        if (await _context.Users.AnyAsync(u => u.Email == request.NewEmail && u.Id != userId))
            return BaseResponse<string>.ErrorResponse("Email already registered", ErrorCodes.AuthEmailTaken);

        // Create email verification
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");

        var verification = new EmailVerification
        {
            UserId = userId,
            Email = request.NewEmail,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailVerifications.Add(verification);
        await _context.SaveChangesAsync();

        // TODO: Send verification email
        await _auditService.LogAsync(userId, "EmailUpdateRequested", $"New email: {request.NewEmail}");

        return BaseResponse<string>.SuccessResponse("Verification email sent to new address. Please verify to complete email change.");
    }

    public async Task<BaseResponse<string>> DeleteAccountAsync(int userId, DeleteAccountRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BaseResponse<string>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        // Verify password
        if (!PasswordHelper.VerifyHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return BaseResponse<string>.ErrorResponse("Password is incorrect", ErrorCodes.AuthInvalidCredentials);

        if (request.HardDelete)
        {
            // Hard delete - remove all related data
            _context.Users.Remove(user);
            await _auditService.LogAsync(userId, "AccountHardDeleted", null);
        }
        else
        {
            // Soft delete
            user.Status = UserStatus.Deleted;
            await _auditService.LogAsync(userId, "AccountSoftDeleted", null);
        }

        await _context.SaveChangesAsync();

        return BaseResponse<string>.SuccessResponse(request.HardDelete ? "Account permanently deleted" : "Account deleted. You can recover within 30 days.");
    }

    public async Task<BaseResponse<UserStatisticsDto>> GetStatisticsAsync(int userId, bool forceRefresh = false)
    {
        // Cache key: user statistics (expensive calculation)
        var cacheKey = $"user_statistics_{userId}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<UserStatisticsDto>(cacheKey);
            if (cached != null)
                return BaseResponse<UserStatisticsDto>.SuccessResponse(cached);
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return BaseResponse<UserStatisticsDto>.ErrorResponse("User not found", ErrorCodes.AuthUserNotFound);

        var examResults = await _context.ExamResults
            .AsNoTracking()
            .Where(er => er.StudentId == userId && er.IsConfirmed)
            .ToListAsync();

        var totalExams = examResults.Count;
        var averageScore = examResults.Any() ? examResults.Average(er => er.TotalScore) : 0;
        var averageNet = examResults.Any() ? examResults.Average(er => er.TotalNet) : 0;
        var lastExamDate = examResults.Any() ? examResults.Max(er => er.CreatedAt) : (DateTime?)null;
        var bestRank = examResults.Where(er => er.InstitutionRank.HasValue).Any() 
            ? examResults.Where(er => er.InstitutionRank.HasValue).Min(er => er.InstitutionRank) : null;

        // En iyi ders hesaplama
        string? bestLesson = null;
        if (examResults.Any())
        {
            var lessonNets = new Dictionary<string, List<float>>();
            foreach (var result in examResults)
            {
                var detailed = JsonSerializer.Deserialize<Dictionary<string, LessonScore>>(result.DetailedResultsJson ?? "{}");
                if (detailed != null)
                {
                    foreach (var lesson in detailed)
                    {
                        if (!lessonNets.ContainsKey(lesson.Key))
                            lessonNets[lesson.Key] = new List<float>();
                        lessonNets[lesson.Key].Add(lesson.Value.Net);
                    }
                }
            }
            bestLesson = lessonNets.Any() 
                ? lessonNets.OrderByDescending(kvp => kvp.Value.Average()).First().Key 
                : null;
        }

        var totalMessages = await _context.Messages
            .AsNoTracking()
            .CountAsync(m => m.SenderId == userId);
        var totalNotifications = await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId);

        var statistics = new UserStatisticsDto
        {
            TotalExams = totalExams,
            AverageScore = averageScore,
            AverageNet = averageNet,
            BestLesson = bestLesson,
            MostImprovedTopic = null, // TODO: Implement topic improvement calculation
            TotalMessages = totalMessages,
            TotalNotifications = totalNotifications,
            LastExamDate = lastExamDate,
            BestRank = bestRank
        };

        // Cache for 10 minutes (statistics change when new exam results are added)
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, statistics, TimeSpan.FromMinutes(10));
        }

        return BaseResponse<UserStatisticsDto>.SuccessResponse(statistics);
    }

    public async Task<BaseResponse<List<UserActivityDto>>> GetActivityAsync(int userId, int page = 1, int limit = 20)
    {
        var activities = new List<UserActivityDto>();

        // Son girişler
        var user = await _context.Users.FindAsync(userId);
        if (user?.LastLoginAt.HasValue == true)
        {
            activities.Add(new UserActivityDto
            {
                Type = "Login",
                Description = "Son giriş",
                CreatedAt = user.LastLoginAt.Value,
                RelatedEntityType = "User"
            });
        }

        // Son sınav sonuçları
        var recentExams = await _context.ExamResults
            .Where(er => er.StudentId == userId && er.IsConfirmed)
            .OrderByDescending(er => er.CreatedAt)
            .Take(limit)
            .Select(er => new UserActivityDto
            {
                Type = "Exam",
                Description = $"{er.Exam.Title} - Net: {er.TotalNet:F2}",
                CreatedAt = er.CreatedAt,
                RelatedEntityId = er.ExamId.ToString(),
                RelatedEntityType = "Exam"
            })
            .ToListAsync();

        activities.AddRange(recentExams);

        // Son mesajlar
        var recentMessages = await _context.Messages
            .Where(m => m.SenderId == userId)
            .OrderByDescending(m => m.SentAt)
            .Take(limit)
            .Select(m => new UserActivityDto
            {
                Type = "Message",
                Description = $"Mesaj gönderildi",
                CreatedAt = m.SentAt,
                RelatedEntityId = m.Id.ToString(),
                RelatedEntityType = "Message"
            })
            .ToListAsync();

        activities.AddRange(recentMessages);

        // Sırala ve pagination
        var sorted = activities.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToList();

        return BaseResponse<List<UserActivityDto>>.SuccessResponse(sorted);
    }

    public async Task<BaseResponse<List<UserSearchResultDto>>> SearchUsersAsync(UserSearchRequest request, int currentUserId)
    {
        var query = _context.Users.AsQueryable();

        // Arama
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            query = query.Where(u => 
                u.Username.Contains(request.Query) ||
                u.FullName.Contains(request.Query) ||
                u.Email.Contains(request.Query));
        }

        // Filtreleme
        if (request.Role.HasValue)
        {
            query = query.Where(u => u.GlobalRole == (UserRole)request.Role.Value);
        }

        if (request.InstitutionId.HasValue)
        {
            var institutionUserIds = await _context.InstitutionUsers
                .Where(iu => iu.InstitutionId == request.InstitutionId.Value)
                .Select(iu => iu.UserId)
                .ToListAsync();
            query = query.Where(u => institutionUserIds.Contains(u.Id));
        }

        // ProfileVisibility kontrolü için current user bilgisi
        var isTeacher = await _context.InstitutionUsers
            .AsNoTracking()
            .AnyAsync(iu =>
                iu.UserId == currentUserId &&
                iu.Role == InstitutionRole.Teacher);

        var users = await query
            .AsNoTracking() // Read-only, optimize
            .Include(u => u.InstitutionMemberships)
                .ThenInclude(im => im.Institution)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Username,
                u.ProfileImageUrl,
                u.GlobalRole,
                u.ProfileVisibility,
                Institutions = u.InstitutionMemberships.Select(im => new InstitutionSummaryDto
                {
                    Id = im.InstitutionId,
                    Name = im.Institution.Name,
                    Role = im.Role.ToString()
                }).ToList()
            })
            .ToListAsync();

        var results = users.Select(u => new UserSearchResultDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Username = u.Username,
            ProfileImageUrl = u.ProfileImageUrl,
            GlobalRole = u.GlobalRole.ToString(),
            Institutions = u.Institutions,
            IsVisible = u.Id == currentUserId || 
                       u.ProfileVisibility == ProfileVisibility.PublicToAll ||
                       (u.ProfileVisibility == ProfileVisibility.TeachersOnly && isTeacher)
        }).ToList();

        return BaseResponse<List<UserSearchResultDto>>.SuccessResponse(results);
    }

    public async Task<BaseResponse<UserPreferencesDto>> GetPreferencesAsync(int userId, bool forceRefresh = false)
    {
        // Cache key: user preferences (frequently read, rarely changed)
        var cacheKey = $"user_preferences_{userId}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<UserPreferencesDto>(cacheKey);
            if (cached != null)
                return BaseResponse<UserPreferencesDto>.SuccessResponse(cached);
        }

        var preferences = await _context.UserPreferences
            .AsNoTracking() // Read-only
            .FirstOrDefaultAsync(up => up.UserId == userId);

        if (preferences == null)
        {
            // Create default preferences
            preferences = new UserPreferences
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.UserPreferences.Add(preferences);
            await _context.SaveChangesAsync();
        }

        var dto = new UserPreferencesDto
        {
            Theme = preferences.Theme,
            Language = preferences.Language,
            DateFormat = preferences.DateFormat,
            TimeFormat = preferences.TimeFormat,
            EmailNotifications = preferences.EmailNotifications,
            PushNotifications = preferences.PushNotifications,
            ExamResultNotifications = preferences.ExamResultNotifications,
            MessageNotifications = preferences.MessageNotifications,
            AccountLinkNotifications = preferences.AccountLinkNotifications,
            ProfileLayout = preferences.ProfileLayout,
            ShowStatistics = preferences.ShowStatistics,
            ShowActivity = preferences.ShowActivity,
            ShowAchievements = preferences.ShowAchievements,
            DashboardLayout = preferences.DashboardLayout,
            VisibleWidgets = preferences.VisibleWidgets
        };

        // Cache for 30 minutes (preferences rarely change)
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));
        }

        return BaseResponse<UserPreferencesDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<string>> UpdatePreferencesAsync(int userId, UserPreferencesDto request)
    {
        var preferences = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userId);

        if (preferences == null)
        {
            preferences = new UserPreferences { UserId = userId };
            _context.UserPreferences.Add(preferences);
        }

        preferences.Theme = request.Theme;
        preferences.Language = request.Language;
        preferences.DateFormat = request.DateFormat;
        preferences.TimeFormat = request.TimeFormat;
        preferences.EmailNotifications = request.EmailNotifications;
        preferences.PushNotifications = request.PushNotifications;
        preferences.ExamResultNotifications = request.ExamResultNotifications;
        preferences.MessageNotifications = request.MessageNotifications;
        preferences.AccountLinkNotifications = request.AccountLinkNotifications;
        preferences.ShowStatistics = request.ShowStatistics;
        preferences.ShowActivity = request.ShowActivity;
        preferences.ShowAchievements = request.ShowAchievements;
        preferences.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.InvalidateUserCacheAsync(userId);

        await _auditService.LogAsync(userId, "PreferencesUpdated", null);

        return BaseResponse<string>.SuccessResponse("Preferences updated successfully");
    }

    public async Task<BaseResponse<string>> UpdateProfileLayoutAsync(int userId, string layoutJson)
    {
        var preferences = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userId);

        if (preferences == null)
        {
            preferences = new UserPreferences { UserId = userId };
            _context.UserPreferences.Add(preferences);
        }

        preferences.ProfileLayout = layoutJson;
        preferences.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.InvalidateUserCacheAsync(userId);

        return BaseResponse<string>.SuccessResponse("Profile layout updated successfully");
    }

    public async Task<BaseResponse<string>> UpdateDashboardLayoutAsync(int userId, string layoutJson, string visibleWidgetsJson)
    {
        var preferences = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userId);

        if (preferences == null)
        {
            preferences = new UserPreferences { UserId = userId };
            _context.UserPreferences.Add(preferences);
        }

        preferences.DashboardLayout = layoutJson;
        preferences.VisibleWidgets = visibleWidgetsJson;
        preferences.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.InvalidateUserCacheAsync(userId);

        return BaseResponse<string>.SuccessResponse("Dashboard layout updated successfully");
    }
}

