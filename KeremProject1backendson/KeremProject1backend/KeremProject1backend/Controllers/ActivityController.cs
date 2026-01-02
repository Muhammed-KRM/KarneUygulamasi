using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KeremProject1backend.Controllers
{
    [Route("api/activity")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ActivityController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetUserActivity(int userId, [FromQuery] int page = 1, [FromQuery] int limit = 20, [FromHeader(Name = "Token")] string? token = null)
        {
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
            }

            var response = await ActivityOperations.GetUserActivity(userId, _context, page, limit, session);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("me")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetMyActivity([FromHeader] string token, [FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            int userId = SessionService.GetUserId(session);
            var response = await ActivityOperations.GetUserActivity(userId, _context, page, limit, session);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

