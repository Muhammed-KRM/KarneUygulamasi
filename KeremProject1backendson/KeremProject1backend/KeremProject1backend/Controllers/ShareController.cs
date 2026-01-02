using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KeremProject1backend.Controllers
{
    [Route("api/share")]
    [ApiController]
    public class ShareController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public ShareController(ApplicationContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpPost("set-visibility")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> SetVisibility([FromBody] SetListVisibilityRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await ShareOperations.SetListVisibility(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("generate-link")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GenerateLink([FromBody] GenerateShareLinkRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await ShareOperations.GenerateShareLink(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("public/{shareToken}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetPublicList(string shareToken, [FromHeader(Name = "Token")] string? token = null)
        {
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
            }

            var response = await ShareOperations.GetPublicListByToken(shareToken, _context, _httpClientFactory, session, _config);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("public")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetPublicLists([FromQuery] int page = 1, [FromQuery] int limit = 20, [FromHeader(Name = "Token")] string? token = null)
        {
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
            }

            var response = await ShareOperations.GetPublicLists(page, limit, _context, session, _config);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpDelete("link/{listId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> DeleteShareLink(int listId, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await ShareOperations.DeleteShareLink(listId, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

