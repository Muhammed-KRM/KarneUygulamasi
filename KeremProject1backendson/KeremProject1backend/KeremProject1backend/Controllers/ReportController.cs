using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/report")]
[Authorize]
public class ReportController : BaseController
{
    private readonly ExamOperations _examOperations;

    public ReportController(ExamOperations examOperations, SessionService sessionService) : base(sessionService)
    {
        _examOperations = examOperations;
    }

    [HttpGet("student/{resultId}")]
    public async Task<IActionResult> GetStudentReport(int resultId)
    {
        var userId = GetCurrentUserId();
        var result = await _examOperations.GetStudentReportAsync(resultId, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("student/{studentId}/all")]
    public async Task<IActionResult> GetStudentAllReports(int studentId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _examOperations.GetStudentAllReportsAsync(studentId, currentUserId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("classroom/{classroomId}")]
    public async Task<IActionResult> GetClassroomReports(int classroomId, [FromQuery] int? examId = null)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _examOperations.GetClassroomReportsAsync(classroomId, examId, currentUserId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

