using KeremProject1backend.Core.Constants;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KeremProject1backend.Operations;

public class AccountOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly NotificationService _notificationService;
    private readonly AuthorizationService _authorizationService;

    public AccountOperations(
        ApplicationContext context, 
        SessionService sessionService, 
        NotificationService notificationService,
        AuthorizationService authorizationService)
    {
        _context = context;
        _sessionService = sessionService;
        _notificationService = notificationService;
        _authorizationService = authorizationService;
    }

    public async Task<BaseResponse<int>> RequestAccountLinkAsync(int institutionId, string studentNumber)
    {
        var currentUserId = _sessionService.GetUserId();

        // 1. Find InstitutionUser by student number
        var institutionUser = await _context.InstitutionUsers
            .FirstOrDefaultAsync(iu =>
                iu.InstitutionId == institutionId &&
                iu.StudentNumber == studentNumber &&
                iu.Role == InstitutionRole.Student);

        if (institutionUser == null)
            return BaseResponse<int>.ErrorResponse("Öğrenci numarası bulunamadı", ErrorCodes.GenericError);

        // 2. Check if link already exists
        var existingLink = await _context.AccountLinks
            .AnyAsync(al =>
                al.MainUserId == currentUserId &&
                al.InstitutionUserId == institutionUser.Id);

        if (existingLink)
            return BaseResponse<int>.ErrorResponse("Zaten bağlantı talebi gönderdiniz", ErrorCodes.GenericError);

        // 3. Create AccountLink
        var accountLink = new AccountLink
        {
            MainUserId = currentUserId,
            InstitutionUserId = institutionUser.Id,
            Status = LinkStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        _context.AccountLinks.Add(accountLink);
        await _context.SaveChangesAsync();

        // 4. Notify institution manager
        var institution = await _context.Institutions
            .Include(i => i.Manager)
            .FirstOrDefaultAsync(i => i.Id == institutionId);

        if (institution != null && institution.ManagerUserId > 0)
        {
            await _notificationService.SendNotificationAsync(
                institution.ManagerUserId,
                "Hesap Bağlama Talebi",
                $"Yeni bir hesap bağlama talebi var. Öğrenci No: {studentNumber}",
                NotificationType.System,
                $"/institution/link-requests"
            );
        }

        return BaseResponse<int>.SuccessResponse(accountLink.Id);
    }

    public async Task<BaseResponse<bool>> ApproveAccountLinkAsync(int linkId)
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

        var accountLink = await _context.AccountLinks
            .Include(al => al.InstitutionUser)
            .ThenInclude(iu => iu.Institution)
            .FirstOrDefaultAsync(al => al.Id == linkId);

        if (accountLink == null)
            return BaseResponse<bool>.ErrorResponse("Bağlantı talebi bulunamadı", ErrorCodes.GenericError);

        if (accountLink.Status != LinkStatus.Pending)
            return BaseResponse<bool>.ErrorResponse("Bu talep zaten işlenmiş", ErrorCodes.GenericError);

        var currentUserId = _sessionService.GetUserId();

        // Approve link
        accountLink.Status = LinkStatus.Approved;
        accountLink.ProcessedAt = DateTime.UtcNow;
        accountLink.ProcessedByUserId = currentUserId;

        await _context.SaveChangesAsync();

        // Notify main user
        await _notificationService.SendNotificationAsync(
            accountLink.MainUserId,
            "Hesap Bağlantısı Onaylandı",
            $"Hesap bağlantı talebiniz onaylandı. Artık kurum hesabınıza erişebilirsiniz.",
            NotificationType.System,
            $"/account/linked-accounts"
        );

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> RejectAccountLinkAsync(int linkId)
    {
        var currentUserId = _sessionService.GetUserId();

        var accountLink = await _context.AccountLinks
            .Include(al => al.InstitutionUser)
            .ThenInclude(iu => iu.Institution)
            .FirstOrDefaultAsync(al => al.Id == linkId);

        if (accountLink == null)
            return BaseResponse<bool>.ErrorResponse("Bağlantı talebi bulunamadı", ErrorCodes.GenericError);

        var isManager = await _context.Institutions
            .AnyAsync(i => i.Id == accountLink.InstitutionUser.InstitutionId && i.ManagerUserId == currentUserId);

        if (!isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<bool>.ErrorResponse("Yetkiniz yok", ErrorCodes.AccessDenied);

        accountLink.Status = LinkStatus.Rejected;
        accountLink.ProcessedAt = DateTime.UtcNow;
        accountLink.ProcessedByUserId = currentUserId;

        await _context.SaveChangesAsync();

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<PagedResult<AccountLinkRequestDto>>> GetLinkRequestsAsync(
        int userId,
        LinkStatus? status = null,
        int page = 1,
        int limit = 20)
    {
        // Check if user is a manager
        var userInstitutions = await _context.InstitutionUsers
            .Where(iu => iu.UserId == userId && iu.Role == InstitutionRole.Manager)
            .Select(iu => iu.InstitutionId)
            .ToListAsync();

        var query = _context.AccountLinks
            .Include(al => al.InstitutionUser)
                .ThenInclude(iu => iu.Institution)
            .Include(al => al.MainUser)
            .Where(al => userInstitutions.Contains(al.InstitutionUser.InstitutionId))
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(al => al.Status == status.Value);

        var total = await query.CountAsync();
        var links = await query
            .OrderByDescending(al => al.RequestedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(al => new AccountLinkRequestDto
            {
                Id = al.Id,
                MainUserName = al.MainUser.FullName,
                InstitutionName = al.InstitutionUser.Institution.Name,
                StudentNumber = al.InstitutionUser.StudentNumber,
                Status = al.Status.ToString(),
                RequestedAt = al.RequestedAt,
                ProcessedAt = al.ProcessedAt
            })
            .ToListAsync();

        var result = new PagedResult<AccountLinkRequestDto>
        {
            Items = links,
            Total = total,
            Page = page,
            Limit = limit,
            TotalPages = (int)Math.Ceiling(total / (double)limit)
        };

        return BaseResponse<PagedResult<AccountLinkRequestDto>>.SuccessResponse(result);
    }

    public async Task<BaseResponse<List<AccountLinkDto>>> GetLinksAsync(int userId)
    {
        var links = await _context.AccountLinks
            .Include(al => al.InstitutionUser)
                .ThenInclude(iu => iu.Institution)
            .Where(al => al.MainUserId == userId && al.Status == LinkStatus.Approved)
            .Select(al => new AccountLinkDto
            {
                Id = al.Id,
                InstitutionId = al.InstitutionUser.InstitutionId,
                InstitutionName = al.InstitutionUser.Institution.Name,
                Role = al.InstitutionUser.Role.ToString(),
                StudentNumber = al.InstitutionUser.StudentNumber,
                LinkedAt = al.ProcessedAt ?? al.RequestedAt
            })
            .ToListAsync();

        return BaseResponse<List<AccountLinkDto>>.SuccessResponse(links);
    }

    public async Task<BaseResponse<string>> DeleteLinkAsync(int linkId, int userId)
    {
        var link = await _context.AccountLinks
            .Include(al => al.InstitutionUser)
                .ThenInclude(iu => iu.Institution)
            .FirstOrDefaultAsync(al => al.Id == linkId);

        if (link == null)
            return BaseResponse<string>.ErrorResponse("Link not found", ErrorCodes.GenericError);

        // Authorization: Only main user or manager can delete
        var isMainUser = link.MainUserId == userId;
        var isManager = await _context.Institutions.AnyAsync(i =>
            i.Id == link.InstitutionUser.InstitutionId && i.ManagerUserId == userId);

        if (!isMainUser && !isManager && !_sessionService.IsInGlobalRole(UserRole.AdminAdmin))
            return BaseResponse<string>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);

        _context.AccountLinks.Remove(link);
        await _context.SaveChangesAsync();

        return BaseResponse<string>.SuccessResponse("Link deleted successfully");
    }
}

public class AccountLinkRequestDto
{
    public int Id { get; set; }
    public string MainUserName { get; set; } = string.Empty;
    public string InstitutionName { get; set; } = string.Empty;
    public string? StudentNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class AccountLinkDto
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? StudentNumber { get; set; }
    public DateTime LinkedAt { get; set; }
}

