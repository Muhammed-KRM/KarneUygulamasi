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
[Authorize]
public class InstitutionController : BaseController
{
    private readonly InstitutionOperations _institutionOperations;
    private readonly AdminOperations _adminOperations;

    public InstitutionController(
        InstitutionOperations institutionOperations,
        AdminOperations adminOperations,
        SessionService sessionService) : base(sessionService)
    {
        _institutionOperations = institutionOperations;
        _adminOperations = adminOperations;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyInstitutions()
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.GetMyInstitutionsAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInstitution(int id, [FromQuery] bool forceRefresh = false)
    {
        var result = await _institutionOperations.GetInstitutionAsync(id, GetCurrentUserId(), forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInstitution(int id, [FromBody] UpdateInstitutionRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.UpdateInstitutionAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(
        int id,
        [FromQuery] InstitutionRole? role = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null)
    {
        var result = await _institutionOperations.GetMembersAsync(id, role, page, limit, search);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/add-member")]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.AddMemberAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}/member/{memberId}")]
    public async Task<IActionResult> RemoveMember(int id, int memberId)
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.RemoveMemberAsync(id, memberId, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}/member/{memberId}/role")]
    public async Task<IActionResult> UpdateMemberRole(int id, int memberId, [FromBody] UpdateMemberRoleRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.UpdateMemberRoleAsync(id, memberId, request.Role, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}/statistics")]
    public async Task<IActionResult> GetStatistics(int id, [FromQuery] bool forceRefresh = false)
    {
        var result = await _institutionOperations.GetStatisticsAsync(id, GetCurrentUserId(), forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/managers")]
    public async Task<IActionResult> CreateManager(int id, [FromBody] CreateManagerRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.CreateManagerAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}/managers/{managerId}")]
    public async Task<IActionResult> UpdateManager(int id, int managerId, [FromBody] UpdateUserRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.UpdateManagerAsync(id, managerId, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}/managers/{managerId}")]
    public async Task<IActionResult> RemoveManager(int id, int managerId)
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.RemoveManagerAsync(id, managerId, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Kuruma ikinci bir sahip (co-owner) ekler
    /// </summary>
    [HttpPost("{id}/add-owner")]
    public async Task<IActionResult> AddOwner(int id, [FromBody] AddOwnerRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.AddOwnerAsync(id, request.UserId, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Kurum için yeni öğretmen hesabı oluşturur
    /// </summary>
    [HttpPost("{id}/create-teacher")]
    public async Task<IActionResult> CreateTeacher(int id, [FromBody] CreateTeacherRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.CreateTeacherAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Kurum için yeni öğrenci hesabı oluşturur
    /// </summary>
    [HttpPost("{id}/create-student")]
    public async Task<IActionResult> CreateStudent(int id, [FromBody] CreateStudentRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _institutionOperations.CreateStudentAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}


public class UpdateMemberRoleRequest
{
    public InstitutionRole Role { get; set; }
}

public class AddOwnerRequest
{
    public int UserId { get; set; }
}

public class CreateTeacherRequest
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? Phone { get; set; }
    public string? EmployeeNumber { get; set; }
}

public class CreateStudentRequest
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? Phone { get; set; }
    public required string StudentNumber { get; set; }
}
