using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KeremProject1backend.Controllers
{
    [Route("api/log")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public LogController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Kullanıcının kendi loglarını getir (veya Admin başka kullanıcının loglarını)
        /// </summary>
        [HttpPost("user")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetUserLogs([FromBody] GetUserLogsRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await LogOperations.GetUserLogs(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Admin için tüm logları getir
        /// </summary>
        [HttpPost("admin/all")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetAdminLogs([FromBody] GetAdminLogsRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await LogOperations.GetAdminLogs(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

