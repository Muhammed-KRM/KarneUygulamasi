using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public AuthController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var response = await AuthOperations.Register(registerDto, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("login")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await AuthOperations.Login(loginDto, _context);
            if (response.Errored) return Unauthorized(response);
            return Ok(response);
        }

        /// <summary>
        /// Admin kullanıcı oluştur - Sadece AdminAdmin kullanabilir
        /// </summary>
        [HttpPost("create-admin")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto, [FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await AuthOperations.CreateAdmin(dto, session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// AdminAdmin kullanıcı oluştur - Hiçbir yetki istemez (sonradan silinecek)
        /// </summary>
        [HttpPost("create-adminadmin")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> CreateAdminAdmin([FromBody] CreateAdminAdminDto dto)
        {
            var response = await AuthOperations.CreateAdminAdmin(dto, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Logout işlemi - Token'ı geçersiz kılmak için
        /// Not: Şu an sadece başarı mesajı döner. Client tarafında token'ı silmek yeterli.
        /// Gelecekte token blacklist mekanizması eklenebilir.
        /// </summary>
        [HttpPost("logout")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public async Task<IActionResult> Logout([FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            var response = await AuthOperations.Logout(session, _context);
            if (response.Errored) return BadRequest(response);
            return Ok(response);
        }
    }
}
