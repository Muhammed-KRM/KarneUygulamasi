using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KeremProject1backend.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CommentController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await CommentOperations.AddComment(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("list/{listId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetComments(int listId, [FromHeader(Name = "Token")] string? token = null)
        {
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
            }

            var response = await CommentOperations.GetComments(listId, _context, session);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("update")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await CommentOperations.UpdateComment(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpDelete("{commentId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> DeleteComment(int commentId, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await CommentOperations.DeleteComment(commentId, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

