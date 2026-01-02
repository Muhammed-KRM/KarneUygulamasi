using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers
{
    [Route("api/dragdrop")]
    [ApiController]
    public class DragDropController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public DragDropController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("move-item")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> MoveItem([FromBody] MoveItemRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await DragDropOperations.MoveItem(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("reorder-items")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> ReorderItems([FromBody] ReorderItemsRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await DragDropOperations.ReorderItems(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

