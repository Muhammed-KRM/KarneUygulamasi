using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers
{
    [Route("api/generate")]
    [ApiController]
    public class ListGeneratorController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public ListGeneratorController(ApplicationContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpPost("by-score")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GenerateByScore([FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await ListGeneratorOperations.GenerateByScore(session, _context, _httpClientFactory, _config);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("by-year")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GenerateByYear([FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await ListGeneratorOperations.GenerateByYear(session, _context, _httpClientFactory, _config);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("by-genre")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GenerateByGenre([FromBody] GenerateByGenreRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await ListGeneratorOperations.GenerateByGenre(request, session, _context, _httpClientFactory, _config);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("genres")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetGenres()
        {
            var response = await ListGeneratorOperations.GetGenres(_httpClientFactory);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}
