using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    private readonly SessionService _sessionService;

    public BaseController(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    protected int GetCurrentUserId()
    {
        return _sessionService.GetCurrentUserId(User);
    }

    protected string? GetInstitutionRole(int institutionId)
    {
        return _sessionService.GetInstitutionRole(User, institutionId);
    }
}
