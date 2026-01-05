using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "AdminAdmin,Admin")]
public class AdminController : BaseController
{
    private readonly ApplicationContext _context;
    private readonly AuditService _auditService;
    private readonly AdminOperations _adminOperations;

    public AdminController(
        ApplicationContext context,
        SessionService sessionService,
        AuditService auditService,
        AdminOperations adminOperations) : base(sessionService)
    {
        _context = context;
        _auditService = auditService;
        _adminOperations = adminOperations;
    }

    [HttpPost("approve-institution/{id}")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> ApproveInstitution(int id)
    {
        var adminId = GetCurrentUserId();
        var result = await AuthOperations.ApproveInstitutionAsync(id, adminId, _context, _auditService);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("pending-institutions")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> GetPendingInstitutions()
    {
        var result = await AuthOperations.GetPendingInstitutionsAsync(_context);
        return Ok(result);
    }

    // Kullanıcı Yönetimi
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] UserStatus? status = null,
        [FromQuery] UserRole? role = null,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _adminOperations.GetAllUsersAsync(page, limit, search, status, role, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _adminOperations.GetUserAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("users/{id}")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserRequest request)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminOperations.UpdateUserAsync(id, request, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("users/{id}/status")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminOperations.UpdateUserStatusAsync(id, request.Status, adminId, request.Reason);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("users/{id}")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminOperations.DeleteUserAsync(id, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("users/{id}/reset-password")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> ResetUserPassword(int id)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminOperations.ResetUserPasswordAsync(id, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // Kurum Yönetimi
    [HttpGet("institutions")]
    public async Task<IActionResult> GetAllInstitutions(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] InstitutionStatus? status = null,
        [FromQuery] string? search = null)
    {
        var result = await _adminOperations.GetAllInstitutionsAsync(page, limit, status, search);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("institutions/{id}")]
    public async Task<IActionResult> GetInstitution(int id)
    {
        var result = await _adminOperations.GetInstitutionAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("institutions/{id}/reject")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> RejectInstitution(int id, [FromBody] RejectInstitutionRequest request)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminOperations.RejectInstitutionAsync(id, request.Reason, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("institutions/{id}/status")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> UpdateInstitutionStatus(int id, [FromBody] UpdateInstitutionStatusRequest request)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminOperations.UpdateInstitutionStatusAsync(id, request.Status, adminId, request.Reason);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("institutions/{id}/subscription")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> ExtendSubscription(int id, [FromBody] ExtendSubscriptionRequest request)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminOperations.ExtendSubscriptionAsync(id, request.Months, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // Admin Hesap Yönetimi
    [HttpPost("create-admin")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminOperations.CreateAdminAsync(request, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("admins")]
    [Authorize(Roles = "AdminAdmin")]
    public async Task<IActionResult> GetAdmins()
    {
        var result = await _adminOperations.GetAdminsAsync();
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // İstatistikler
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] bool forceRefresh = false)
    {
        var result = await _adminOperations.GetStatisticsAsync(forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // Audit Logs
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50)
    {
        var result = await _adminOperations.GetAuditLogsAsync(userId, action, dateFrom, dateTo, page, limit);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("audit-logs/user/{userId}")]
    public async Task<IActionResult> GetUserAuditLogs(int userId, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var result = await _adminOperations.GetAuditLogsAsync(userId, null, null, null, page, limit);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

// Request DTOs
public class UpdateUserStatusRequest
{
    public UserStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class RejectInstitutionRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class UpdateInstitutionStatusRequest
{
    public InstitutionStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class ExtendSubscriptionRequest
{
    public int Months { get; set; }
}
