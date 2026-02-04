using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthOperations _authOperations;
    private readonly SessionService _sessionService;

    public AuthController(
        AuthOperations authOperations,
        SessionService sessionService)
    {
        _authOperations = authOperations;
        _sessionService = sessionService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authOperations.RegisterAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authOperations.LoginAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = _sessionService.GetUserId();

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
        var userId = _sessionService.GetUserId();
        var result = await _authOperations.ApplyInstitutionAsync(request, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authOperations.RefreshTokenAsync(request);
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
