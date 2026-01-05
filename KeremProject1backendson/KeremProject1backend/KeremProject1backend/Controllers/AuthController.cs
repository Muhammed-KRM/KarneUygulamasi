using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Models.DTOs.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly AuditService _auditService;

    public AuthController(
        ApplicationContext context,
        SessionService sessionService,
        AuditService auditService)
    {
        _context = context;
        _sessionService = sessionService;
        _auditService = auditService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await AuthOperations.RegisterAsync(request, _context, _auditService);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await AuthOperations.LoginAsync(request, _context, _sessionService, _auditService);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = _sessionService.GetCurrentUserId(User);

        return Ok(new BaseResponse<object>
        {
            Success = true,
            Data = new { UserId = userId, Claims = User.Claims.Select(c => new { c.Type, c.Value }) }
        });
    }

    [HttpPost("apply-institution")]
    [Authorize]
    public async Task<IActionResult> ApplyInstitution([FromBody] Models.DTOs.Requests.ApplyInstitutionRequest request)
    {
        var userId = _sessionService.GetCurrentUserId(User);
        var result = await AuthOperations.ApplyInstitutionAsync(request, userId, _context, _auditService);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await AuthOperations.RefreshTokenAsync(request, _context, _sessionService, _auditService);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, [FromServices] UserOperations userOperations)
    {
        var result = await userOperations.ForgotPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, [FromServices] UserOperations userOperations)
    {
        var result = await userOperations.ResetPasswordAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
