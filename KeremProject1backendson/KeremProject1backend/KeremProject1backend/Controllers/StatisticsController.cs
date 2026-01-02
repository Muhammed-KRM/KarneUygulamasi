using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KeremProject1backend.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public StatisticsController(ApplicationContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet("user/{userId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetUserStatistics(int userId, [FromHeader(Name = "Token")] string? token = null)
        {
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
            }

            var response = await StatisticsOperations.GetUserStatistics(userId, _context, _httpClientFactory, _config, session);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("me")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetMyStatistics([FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            int userId = SessionService.GetUserId(session);
            var response = await StatisticsOperations.GetUserStatistics(userId, _context, _httpClientFactory, _config, session);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

