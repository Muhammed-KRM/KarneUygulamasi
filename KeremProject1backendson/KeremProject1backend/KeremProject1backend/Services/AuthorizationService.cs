using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Services;

/// <summary>
/// Merkezi yetki kontrolü servisi. Tüm yetki kontrolleri bu servis üzerinden yapılır.
/// </summary>
public class AuthorizationService
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;

    public AuthorizationService(ApplicationContext context, SessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    /// <summary>
    /// Kullanıcının belirli bir global role sahip olup olmadığını kontrol eder.
    /// Session'dan UserId alınır, User tablosundan GlobalRole kontrol edilir.
    /// Yetki yoksa BaseResponse ile hata döndürür.
    /// </summary>
    /// <param name="requiredRoles">Gerekli roller (en az biri olmalı)</param>
    /// <returns>Yetki varsa null, yoksa BaseResponse hatası</returns>
    public virtual BaseResponse<string>? RequireGlobalRole(params UserRole[] requiredRoles)
    {
        var userId = _sessionService.GetUserId();
        var user = _context.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return BaseResponse<string>.ErrorResponse("Kullanıcı bulunamadı", ErrorCodes.AuthUserNotFound);

        if (!requiredRoles.Contains(user.GlobalRole))
        {
            var roleNames = string.Join(" veya ", requiredRoles.Select(r => r.ToString()));
            return BaseResponse<string>.ErrorResponse(
                $"Bu işlem için {roleNames} yetkisi gereklidir",
                ErrorCodes.AccessDenied);
        }

        return null; // Yetki var
    }

    /// <summary>
    /// Checks if user has access to the institution (Owner or Manager)
    /// </summary>
    public virtual async Task<bool> HasInstitutionAccessAsync(int userId, int institutionId)
    {
        // 1. Check if user is an owner
        var isOwner = await _context.InstitutionOwners.AnyAsync(
            o => o.InstitutionId == institutionId && o.UserId == userId);
        if (isOwner) return true;

        // 2. Check if user is a manager
        var isManager = await _context.InstitutionUsers.AnyAsync(
            iu => iu.InstitutionId == institutionId &&
                  iu.UserId == userId &&
                  iu.Role == InstitutionRole.Manager &&
                  iu.IsActive);

        return isManager;
    }
}
