using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : BaseController
{
    private readonly ApplicationContext _context;
    private readonly AuditService _auditService;

    public AdminController(
        ApplicationContext context,
        SessionService sessionService,
        AuditService auditService) : base(sessionService)
    {
        _context = context;
        _auditService = auditService;
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
}
