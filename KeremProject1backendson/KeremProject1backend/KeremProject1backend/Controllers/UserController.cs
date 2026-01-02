using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KeremProject1backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;

        public UserController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Kullanıcı profil resmi yükleme
        /// </summary>
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        [ApiExplorerSettings(IgnoreApi = true)] // Swagger'dan gizle - IFormFile Swagger'da 500 hatası veriyor
        public async Task<IActionResult> UploadUserImage([FromHeader(Name = "Token")] string? token = null, [FromForm] IFormFile? file = null)
        {
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token gerekli."));

            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await UserOperations.UploadUserImage(session, file, _context, _configuration);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Kullanıcı bilgilerini getir (profil resmi linki dahil)
        /// </summary>
        [HttpGet("{userId}")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetUser(int userId, [FromHeader(Name = "Token")] string? token = null)
        {
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
            }

            var response = await UserOperations.GetUser(userId, session, _context, _configuration);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Kendi profil bilgilerimi getir
        /// </summary>
        [HttpGet("me")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetMyProfile([FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            int userId = SessionService.GetUserId(session);
            var response = await UserOperations.GetUser(userId, session, _context, _configuration);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Tüm kullanıcıları getir (pagination ile)
        /// </summary>
        [HttpGet("all")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersRequest request, [FromHeader(Name = "Token")] string? token = null)
        {
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
            }

            var response = await UserOperations.GetAllUsers(request, session, _context, _configuration);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Kullanıcı ara
        /// </summary>
        [HttpPost("search")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> SearchUsers([FromBody] SearchUsersRequest request, [FromHeader(Name = "Token")] string? token = null)
        {
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
            }

            var response = await UserOperations.SearchUsers(request, session, _context, _configuration);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Kullanıcı bilgilerini güncelle
        /// </summary>
        [HttpPut("update")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await UserOperations.UpdateUser(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Şifre değiştir
        /// </summary>
        [HttpPost("change-password")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await UserOperations.ChangePassword(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Profil güncelle
        /// </summary>
        [HttpPut("profile")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await UserOperations.UpdateProfile(request, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Kullanıcı sil (soft delete veya hard delete)
        /// </summary>
        [HttpDelete]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await UserOperations.DeleteUser(request, session, _context, _configuration);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}

