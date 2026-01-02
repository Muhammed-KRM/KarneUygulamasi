using KeremProject1backend.Models.Responses;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KeremProject1backend.Controllers
{
    [Route("api/file")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public FileController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// Güvenli dosya indirme endpoint'i
        /// </summary>
        [HttpGet("download")]
        public IActionResult DownloadFile(
            [FromQuery] string filename,
            [FromQuery] int type,
            [FromQuery] string sessionno,
            [FromQuery] string signature,
            [FromHeader(Name = "Token")] string? token = null)
        {
            // Token kontrolü (opsiyonel, public dosyalar için gerekli olmayabilir)
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
            }

            // Dosya tipi kontrolü
            if (!Enum.IsDefined(typeof(FileType), type))
                return BadRequest("Geçersiz dosya tipi.");

            FileType fileType = (FileType)type;

            // Secret key ile imza doğrulama
            string secretKey = _configuration["AppSettings:FileSettings:FileSecretKey"] ?? "ANIME_RANKER_FILE_SECRET_KEY_2024";
            // ValidateFileLinkSignature metodunu secret key ile güncellemek gerekebilir
            // Şimdilik basit kontrol yapıyoruz
            string expectedSignature = GenerateSignatureForValidation(filename, fileType, sessionno, secretKey);
            if (!expectedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase))
                return Unauthorized("Geçersiz dosya linki.");

            // Dosya yolu
            string baseDirectory = _configuration["AppSettings:FileSettings:BaseDirectory"] ?? "Files";
            string filePath = FileService.GetFilePath(filename, fileType, baseDirectory);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Dosya bulunamadı.");

            // Dosya içeriğini döndür
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            string contentType = GetContentType(filename);

            return File(fileBytes, contentType, filename);
        }

        /// <summary>
        /// İmza doğrulama için signature oluştur
        /// </summary>
        private string GenerateSignatureForValidation(string fileName, FileType fileType, string userId, string secretKey)
        {
            string data = $"{fileName}_{fileType}_{userId}_{secretKey}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes).Substring(0, 16).Replace("+", "-").Replace("/", "_");
            }
        }

        /// <summary>
        /// Temp dosyaları temizle (admin endpoint)
        /// </summary>
        [HttpPost("clean-temp")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public IActionResult CleanTempFiles([FromHeader] string token)
        {
            var session = SessionService.TestToken(token);
            if (session == null)
                return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));

            // Admin kontrolü (opsiyonel)
            // if (!SessionService.isAuthorized(session, UserRole.Admin))
            //     return Unauthorized(new BaseResponse().GenerateError(1001, "Yetkisiz işlem."));

            string tempDirectory = _configuration["AppSettings:FileSettings:TempDirectory"] ?? "Files/Temp";
            int expirationHours = int.Parse(_configuration["AppSettings:FileSettings:TempFileExpirationHours"] ?? "24");
            
            int deletedCount = FileService.CleanOldTempFiles(tempDirectory, expirationHours);

            var response = new BaseResponse();
            response.Response = new { DeletedCount = deletedCount };
            return Ok(response.GenerateSuccess($"{deletedCount} temp dosya temizlendi."));
        }

        /// <summary>
        /// Dosya bilgilerini getir
        /// </summary>
        [HttpGet("info")]
        [Produces("application/json", Type = typeof(BaseResponse))]
        public IActionResult GetFileInfo(
            [FromQuery] string filename,
            [FromQuery] int type,
            [FromHeader(Name = "Token")] string? token = null)
        {
            ClaimsPrincipal? session = null;
            if (!string.IsNullOrEmpty(token))
            {
                session = SessionService.TestToken(token);
                if (session == null)
                    return Unauthorized(new BaseResponse().GenerateError(1000, "Token geçersiz."));
            }

            if (!Enum.IsDefined(typeof(FileType), type))
                return BadRequest(new BaseResponse().GenerateError(2020, "Geçersiz dosya tipi."));

            FileType fileType = (FileType)type;
            string baseDirectory = _configuration["AppSettings:FileSettings:BaseDirectory"] ?? "Files";
            var fileInfo = FileService.GetFileInfo(filename, fileType, baseDirectory);

            if (fileInfo == null)
                return NotFound(new BaseResponse().GenerateError(2021, "Dosya bulunamadı."));

            var response = new BaseResponse();
            response.Response = fileInfo;
            return Ok(response.GenerateSuccess("Dosya bilgileri başarıyla getirildi."));
        }

        /// <summary>
        /// Dosya adına göre content type belirle
        /// </summary>
        private string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}

