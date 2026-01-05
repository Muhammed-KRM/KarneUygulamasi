using KeremProject1backend.Controllers;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : BaseController
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly CacheService _cacheService;

    public NotificationController(ApplicationContext context, SessionService sessionService, CacheService cacheService) : base(sessionService)
    {
        _context = context;
        _sessionService = sessionService;
        _cacheService = cacheService;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] NotificationType? type = null,
        [FromQuery] bool? isRead = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50,
        [FromQuery] bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();
        
        // Cache key with filters
        var cacheKey = $"User:{userId}:Notifications:{type}_{isRead}_{dateFrom?.ToString("yyyyMMdd")}_{dateTo?.ToString("yyyyMMdd")}_{page}_{limit}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<object>(cacheKey);
            if (cached != null)
            {
                return Ok(BaseResponse<object>.SuccessResponse(cached));
            }
        }

        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(n => n.Type == type.Value);

        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);

        if (dateFrom.HasValue)
            query = query.Where(n => n.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(n => n.CreatedAt <= dateTo.Value);

        var total = await query.CountAsync();
        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                ActionUrl = n.ActionUrl,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToListAsync();

        var unreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        var result = new { unreadCount, total, page, limit, notifications };
        
        // Cache for 2 minutes (notifications change frequently)
        if (!forceRefresh)
        {
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));
        }

        return Ok(BaseResponse<object>.SuccessResponse(result));
    }

    [HttpPost("mark-read/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _sessionService.GetUserId();
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notification == null) return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        // Invalidate notifications cache
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Notifications:*");

        return Ok(BaseResponse<bool>.SuccessResponse(true));
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _sessionService.GetUserId();
        
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));

        // Invalidate notifications cache
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Notifications:*");

        return Ok(BaseResponse<bool>.SuccessResponse(true));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var userId = _sessionService.GetUserId();
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notification == null) return NotFound();

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        // Invalidate notifications cache
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Notifications:*");

        return Ok(BaseResponse<bool>.SuccessResponse(true));
    }

    [HttpDelete("clear-all")]
    public async Task<IActionResult> ClearAll()
    {
        var userId = _sessionService.GetUserId();
        
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();

        // Invalidate notifications cache
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Notifications:*");

        return Ok(BaseResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var userId = _sessionService.GetUserId();
        
        // Get from UserPreferences
        var preferences = await _context.UserPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        if (preferences == null)
        {
            // Return defaults
            return Ok(BaseResponse<NotificationSettingsDto>.SuccessResponse(new NotificationSettingsDto
            {
                EmailNotifications = true,
                PushNotifications = true,
                ExamResultNotifications = true,
                MessageNotifications = true,
                AccountLinkNotifications = true
            }));
        }

        var settings = new NotificationSettingsDto
        {
            EmailNotifications = preferences.EmailNotifications,
            PushNotifications = preferences.PushNotifications,
            ExamResultNotifications = preferences.ExamResultNotifications,
            MessageNotifications = preferences.MessageNotifications,
            AccountLinkNotifications = preferences.AccountLinkNotifications
        };

        return Ok(BaseResponse<NotificationSettingsDto>.SuccessResponse(settings));
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] NotificationSettingsDto request)
    {
        var userId = _sessionService.GetUserId();
        
        var preferences = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userId);

        if (preferences == null)
        {
            preferences = new UserPreferences { UserId = userId };
            _context.UserPreferences.Add(preferences);
        }

        preferences.EmailNotifications = request.EmailNotifications;
        preferences.PushNotifications = request.PushNotifications;
        preferences.ExamResultNotifications = request.ExamResultNotifications;
        preferences.MessageNotifications = request.MessageNotifications;
        preferences.AccountLinkNotifications = request.AccountLinkNotifications;
        preferences.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"user_preferences_{userId}");

        return Ok(BaseResponse<string>.SuccessResponse("Notification settings updated successfully"));
    }
}

public class NotificationSettingsDto
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool ExamResultNotifications { get; set; } = true;
    public bool MessageNotifications { get; set; } = true;
    public bool AccountLinkNotifications { get; set; } = true;
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
