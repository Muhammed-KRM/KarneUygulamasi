using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : BaseController
{
    private readonly AccountOperations _accountOperations;

    public AccountController(AccountOperations accountOperations, SessionService sessionService) : base(sessionService)
    {
        _accountOperations = accountOperations;
    }

    [HttpPost("link-request")]
    public async Task<IActionResult> RequestAccountLink([FromBody] AccountLinkRequest request)
    {
        var result = await _accountOperations.RequestAccountLinkAsync(request.InstitutionId, request.StudentNumber);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("link-approve/{id}")]
    public async Task<IActionResult> ApproveAccountLink(int id)
    {
        var result = await _accountOperations.ApproveAccountLinkAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("link-reject/{id}")]
    public async Task<IActionResult> RejectAccountLink(int id)
    {
        var result = await _accountOperations.RejectAccountLinkAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("link-requests")]
    public async Task<IActionResult> GetLinkRequests(
        [FromQuery] LinkStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _accountOperations.GetLinkRequestsAsync(userId, status, page, limit);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("links")]
    public async Task<IActionResult> GetLinks()
    {
        var userId = GetCurrentUserId();
        var result = await _accountOperations.GetLinksAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("link/{id}")]
    public async Task<IActionResult> DeleteLink(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _accountOperations.DeleteLinkAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

public class AccountLinkRequest
{
    public int InstitutionId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
}

