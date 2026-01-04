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
}
