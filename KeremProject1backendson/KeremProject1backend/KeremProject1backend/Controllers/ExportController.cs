using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers
{
    [Route("api/export")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ExportController(ApplicationContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("image/{listId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> ExportAsImage(int listId, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await ExportOperations.ExportListAsImage(listId, session, _context, _httpClientFactory, _configuration);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("embed/{listId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetEmbedCode(int listId)
        {
            var response = await ExportOperations.GetEmbedCode(listId, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

