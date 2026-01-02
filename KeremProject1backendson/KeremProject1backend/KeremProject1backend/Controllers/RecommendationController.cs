using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers
{
    [Route("api/recommendation")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public RecommendationController(ApplicationContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet("anime")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetRecommendations([FromHeader] string token, [FromQuery] int limit = 10)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await RecommendationOperations.GetRecommendations(session, _context, _httpClientFactory, _config, limit);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("trending")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetTrendingLists([FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            var response = await RecommendationOperations.GetTrendingLists(_context, page, limit);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

