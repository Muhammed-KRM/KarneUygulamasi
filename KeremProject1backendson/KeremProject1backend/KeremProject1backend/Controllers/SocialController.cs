using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KeremProject1backend.Controllers
{
    [Route("api/social")]
    [ApiController]
    public class SocialController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;

        public SocialController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("like/{listId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> LikeList(int listId, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await SocialOperations.LikeList(listId, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("follow")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> FollowUser([FromBody] FollowUserRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await SocialOperations.FollowUser(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("profile/{userId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetUserProfile(int userId, [FromHeader(Name = "Token")] string? token = null)
        {
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
            }

            var response = await SocialOperations.GetUserProfile(userId, _context, _configuration, session);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("notifications")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetNotifications([FromHeader] string token, [FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await SocialOperations.GetNotifications(session, _context, page, limit);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("notifications/{notificationId}/read")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await SocialOperations.MarkNotificationAsRead(notificationId, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("notifications/read-all")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> MarkAllNotificationsAsRead([FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await SocialOperations.MarkAllNotificationsAsRead(session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("template/create")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await SocialOperations.CreateTemplate(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("templates")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetTemplates([FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            var response = await SocialOperations.GetTemplates(_context, page, limit);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpDelete("notification/{notificationId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> DeleteNotification(int notificationId, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await SocialOperations.DeleteNotification(notificationId, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpDelete("notifications/all")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> DeleteAllNotifications([FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await SocialOperations.DeleteAllNotifications(session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpDelete("template/{templateId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> DeleteTemplate(int templateId, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await SocialOperations.DeleteTemplate(templateId, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

