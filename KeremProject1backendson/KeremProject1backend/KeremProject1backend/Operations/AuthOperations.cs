using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KeremProject1backend.Operations
{
    public static class AuthOperations
    {
        public static async Task<BaseResponse> Register(RegisterDto dto, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                if (await context.AppUsers.AnyAsync(u => u.UserName.ToLower() == dto.Username.ToLower()))
                {
                    return response.GenerateError(1001, "Bu kullanıcı adı zaten alınmış.");
                }

                CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new AppUser
                {
                    UserName = dto.Username.ToLower(),
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    UserRole = UserRole.User,
                    State = false,
                    ModTime = DateTime.Now,
                    ModUser = 0
                };

                await context.AppUsers.AddAsync(user);
                await context.SaveChangesAsync();

                return response.GenerateSuccess("Kullanıcı başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9999, $"Beklenmeyen hata: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> Login(LoginDto dto, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                var user = await context.AppUsers.FirstOrDefaultAsync(u => u.UserName.ToLower() == dto.Username.ToLower());

                if (user == null || !VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return response.GenerateError(1002, "Kullanıcı adı veya parola hatalı.");
                }

                // Giriş başarılı, token oluştur
                var token = TokenServices.GenerateToken(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Role = user.UserRole.ToString(),
                    Token = token
                };

                response.Response = userDto;
                response.SetUserID(user.Id);
                return response.GenerateSuccess("Giriş başarılı.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9999, $"Beklenmeyen hata: {ex.Message}");
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Admin kullanıcı oluştur - Sadece AdminAdmin kullanabilir
        /// </summary>
        public static async Task<BaseResponse> CreateAdmin(CreateAdminDto dto, ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                // Sadece AdminAdmin yetkisi kontrolü
                if (!SessionService.isAuthorized(session, UserRole.AdminAdmin))
                {
                    return response.GenerateError(1003, "Bu işlem için AdminAdmin yetkisi gereklidir.");
                }

                int currentUserId = SessionService.GetUserId(session);

                if (await context.AppUsers.AnyAsync(u => u.UserName.ToLower() == dto.Username.ToLower()))
                {
                    return response.GenerateError(1001, "Bu kullanıcı adı zaten alınmış.");
                }

                CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new AppUser
                {
                    UserName = dto.Username.ToLower(),
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    UserRole = UserRole.Admin,
                    State = true,
                    ModTime = DateTime.Now,
                    ModUser = currentUserId
                };

                await context.AppUsers.AddAsync(user);
                await context.SaveChangesAsync();

                response.SetUserID(currentUserId);
                return response.GenerateSuccess("Admin kullanıcı başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9999, $"Beklenmeyen hata: {ex.Message}");
            }
        }

        /// <summary>
        /// AdminAdmin kullanıcı oluştur - Hiçbir yetki istemez (sonradan silinecek)
        /// </summary>
        public static async Task<BaseResponse> CreateAdminAdmin(CreateAdminAdminDto dto, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                if (await context.AppUsers.AnyAsync(u => u.UserName.ToLower() == dto.Username.ToLower()))
                {
                    return response.GenerateError(1001, "Bu kullanıcı adı zaten alınmış.");
                }

                CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new AppUser
                {
                    UserName = dto.Username.ToLower(),
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    UserRole = UserRole.AdminAdmin,
                    State = true,
                    ModTime = DateTime.Now,
                    ModUser = 0 // İlk AdminAdmin, ModUser yok
                };

                await context.AppUsers.AddAsync(user);
                await context.SaveChangesAsync();

                return response.GenerateSuccess("AdminAdmin kullanıcı başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9999, $"Beklenmeyen hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Logout işlemi - Token'ı geçersiz kılmak için (şu an sadece başarı mesajı döner)
        /// Gelecekte token blacklist mekanizması eklenebilir
        /// </summary>
        public static Task<BaseResponse> Logout(ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                // Şu an sadece başarı mesajı döner
                // Gelecekte token blacklist mekanizması eklenebilir:
                // - Token'ı veritabanında veya cache'de blacklist'e ekle
                // - Her token doğrulamasında blacklist kontrolü yap

                response.SetUserID(userId);
                return Task.FromResult(response.GenerateSuccess("Başarıyla çıkış yapıldı."));
            }
            catch (Exception ex)
            {
                return Task.FromResult(response.GenerateError(9999, $"Beklenmeyen hata: {ex.Message}"));
            }
        }
    }
}
