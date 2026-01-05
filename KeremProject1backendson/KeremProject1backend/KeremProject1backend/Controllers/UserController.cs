using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Models.DTOs.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController : BaseController
{
    private readonly UserOperations _userOperations;

    public UserController(UserOperations userOperations, SessionService sessionService) : base(sessionService)
    {
        _userOperations = userOperations;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.GetProfileAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.UpdateProfileAsync(userId, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.ChangePasswordAsync(userId, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("upload-profile-image")]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.UploadProfileImageAsync(userId, file);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var result = await _userOperations.LogoutAsync(userId, token);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("send-verification-email")]
    public async Task<IActionResult> SendVerificationEmail()
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.SendVerificationEmailAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var result = await _userOperations.VerifyEmailAsync(token);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("profile/{userId}")]
    public async Task<IActionResult> GetUserProfile(int userId, [FromQuery] bool forceRefresh = false)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _userOperations.GetUserProfileAsync(userId, currentUserId, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.UpdateEmailAsync(userId, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.DeleteAccountAsync(userId, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] bool forceRefresh = false)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.GetStatisticsAsync(userId, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.GetActivityAsync(userId, page, limit);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] UserSearchRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _userOperations.SearchUsersAsync(request, currentUserId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences([FromQuery] bool forceRefresh = false)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.GetPreferencesAsync(userId, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UserPreferencesDto request)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.UpdatePreferencesAsync(userId, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("preferences/profile-layout")]
    public async Task<IActionResult> UpdateProfileLayout([FromBody] UpdateLayoutRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.UpdateProfileLayoutAsync(userId, request.LayoutJson);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("preferences/dashboard-layout")]
    public async Task<IActionResult> UpdateDashboardLayout([FromBody] UpdateDashboardLayoutRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _userOperations.UpdateDashboardLayoutAsync(userId, request.LayoutJson, request.VisibleWidgetsJson);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

public class UpdateLayoutRequest
{
    public string LayoutJson { get; set; } = "{}";
}

public class UpdateDashboardLayoutRequest
{
    public string LayoutJson { get; set; } = "{}";
    public string VisibleWidgetsJson { get; set; } = "[]";
}

