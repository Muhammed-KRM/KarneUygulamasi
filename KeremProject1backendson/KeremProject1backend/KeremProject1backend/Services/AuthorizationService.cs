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
    public BaseResponse<string>? RequireGlobalRole(params UserRole[] requiredRoles)
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
}
