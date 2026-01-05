using KeremProject1backend.Operations;
using KeremProject1backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeremProject1backend.Services;

namespace KeremProject1backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClassroomController : BaseController
{
    private readonly ClassroomOperations _classroomOperations;

    public ClassroomController(ClassroomOperations classroomOperations, SessionService sessionService) : base(sessionService)
    {
        _classroomOperations = classroomOperations;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateClassroom([FromBody] CreateClassroomRequest request)
    {
        var result = await _classroomOperations.CreateClassroomAsync(request.InstitutionId, request.Name, request.Grade);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetails(int id, [FromQuery] bool forceRefresh = false)
    {
        var result = await _classroomOperations.GetClassroomDetailsAsync(id, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("add-student")]
    public async Task<IActionResult> AddStudent([FromBody] AddStudentToClassroomRequest request)
    {
        var result = await _classroomOperations.AddStudentToClassroomAsync(request.ClassroomId, request.StudentId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("add-students-bulk")]
    public async Task<IActionResult> AddStudentsBulk([FromBody] BulkAddStudentsRequest request)
    {
        var result = await _classroomOperations.AddStudentsToClassroomAsync(request.ClassroomId, request.StudentIds);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("institution/{institutionId}")]
    public async Task<IActionResult> GetClassrooms(int institutionId, [FromQuery] bool forceRefresh = false)
    {
        var result = await _classroomOperations.GetClassroomsAsync(institutionId, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClassroom(int id, [FromBody] UpdateClassroomRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _classroomOperations.UpdateClassroomAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClassroom(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _classroomOperations.DeleteClassroomAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{classroomId}/student/{studentId}")]
    public async Task<IActionResult> RemoveStudent(int classroomId, int studentId)
    {
        var userId = GetCurrentUserId();
        var result = await _classroomOperations.RemoveStudentAsync(classroomId, studentId, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{classroomId}/students")]
    public async Task<IActionResult> GetStudents(
        int classroomId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50,
        [FromQuery] string? search = null)
    {
        var result = await _classroomOperations.GetStudentsAsync(classroomId, page, limit, search);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}


public class CreateClassroomRequest
{
    public int InstitutionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Grade { get; set; }
}

public class AddStudentToClassroomRequest
{
    public int ClassroomId { get; set; }
    public int StudentId { get; set; }
}

public class BulkAddStudentsRequest
{
    public int ClassroomId { get; set; }
    public List<int> StudentIds { get; set; } = new();
}
