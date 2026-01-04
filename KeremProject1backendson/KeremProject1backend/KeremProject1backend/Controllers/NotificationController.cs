using KeremProject1backend.Controllers;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DTOs;
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

    public NotificationController(ApplicationContext context, SessionService sessionService) : base(sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyNotifications(int count = 50)
    {
        var userId = _sessionService.GetUserId();
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToListAsync();

        var unreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        return Ok(BaseResponse<object>.SuccessResponse(new { unreadCount, notifications }));
    }

    [HttpPost("mark-read/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _sessionService.GetUserId();
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notification == null) return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return Ok(BaseResponse<bool>.SuccessResponse(true));
    }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
