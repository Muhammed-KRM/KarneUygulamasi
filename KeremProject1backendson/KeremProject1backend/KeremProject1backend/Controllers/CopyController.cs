using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers
{
    [Route("api/copy")]
    [ApiController]
    public class CopyController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CopyController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("list")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> CopyList([FromBody] CopyListRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await CopyOperations.CopyList(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

