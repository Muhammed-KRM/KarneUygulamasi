using KeremProject1backend.Operations;
using KeremProject1backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeremProject1backend.Services;

namespace KeremProject1backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExamController : BaseController
{
    private readonly ExamOperations _examOperations;

    public ExamController(ExamOperations examOperations, SessionService sessionService) : base(sessionService)
    {
        _examOperations = examOperations;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamDto request)
    {
        var result = await _examOperations.CreateExamAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{examId}/process-optical")]
    public async Task<IActionResult> ProcessOptical(int examId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(BaseResponse<bool>.ErrorResponse("File is empty", "300001"));

        using var stream = file.OpenReadStream();
        var result = await _examOperations.ProcessOpticalResultsAsync(examId, stream);

        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmResults(int id)
    {
        var result = await _examOperations.ConfirmResultsAndNotifyAsync(id);
        return Ok(result);
    }
}
