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
    public static class UserOperations
    {
        /// <summary>
        /// Kullanıcı profil resmi yükleme - Basit ve güvenilir versiyon
        /// </summary>
        public static async Task<BaseResponse> UploadUserImage(
            ClaimsPrincipal session,
            IFormFile? file,
            ApplicationContext context,
            IConfiguration configuration)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                // Dosya kontrolü
                if (file == null || file.Length == 0)
                    return response.GenerateError(2001, "Dosya seçilmedi.");

                // Dosya tipi kontrolü
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return response.GenerateError(2002, "Geçersiz dosya tipi. Sadece resim dosyaları kabul edilir (JPG, JPEG, PNG, GIF, WEBP).");

                // Dosya boyutu kontrolü (max 5MB)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (file.Length > maxFileSize)
                    return response.GenerateError(2003, "Dosya boyutu çok büyük. Maksimum 5MB.");

                // Kullanıcıyı bul
                var user = await context.AppUsers.FindAsync(userId);
                if (user == null)
                    return response.GenerateError(2004, "Kullanıcı bulunamadı.");

                // Dosya dizinlerini al
                string baseDirectory = configuration["AppSettings:FileSettings:BaseDirectory"] ?? "Files";
                string userDirectory = Path.Combine(baseDirectory, "Users");

                // Dizin yoksa oluştur
                if (!Directory.Exists(userDirectory))
                    Directory.CreateDirectory(userDirectory);

                // Eski profil resmini sil (varsa)
                if (!string.IsNullOrEmpty(user.UserImageLink))
                {
                    string oldFilePath = FileService.GetFilePath(user.UserImageLink, FileType.User, baseDirectory);
                    if (File.Exists(oldFilePath))
                    {
                        try
                        {
                            File.Delete(oldFilePath);
                        }
                        catch
                        {
                            // Eski dosya silinemezse devam et
                        }
                    }
                }

                // Yeni dosya adı oluştur: userId_timestamp_originalname
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string safeFileName = Path.GetFileNameWithoutExtension(file.FileName)
                    .Replace(" ", "_")
                    .Replace("..", "")
                    .Replace("/", "_")
                    .Replace("\\", "_");
                safeFileName = safeFileName.Length > 50 ? safeFileName.Substring(0, 50) : safeFileName;
                string newFileName = $"{userId.ToString().PadLeft(7, '0')}_{timestamp}_{safeFileName}{fileExtension}";
                string filePath = Path.Combine(userDirectory, newFileName);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Kullanıcı bilgisini güncelle
                user.UserImageLink = newFileName;
                user.ModTime = DateTime.Now;
                user.ModUser = userId;

                await context.SaveChangesAsync();

                // Güvenli download linki oluştur
                string downloadServiceLink = configuration["AppSettings:FileSettings:DownloadServiceLink"] ?? "https://localhost:7132/api/file/download";
                string secretKey = configuration["AppSettings:FileSettings:FileSecretKey"] ?? "ANIME_RANKER_FILE_SECRET_KEY_2024";
                string imageLink = FileService.GenerateFileLinkWithSecret(newFileName, FileType.User, session, downloadServiceLink, secretKey);

                response.Response = new { ImageLink = imageLink, FileName = newFileName };
                response.SetUserID(userId);
                return response.GenerateSuccess("Profil resmi başarıyla yüklendi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(2006, $"Profil resmi yüklenirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Kullanıcı bilgilerini getir
        /// </summary>
        public static async Task<BaseResponse> GetUser(
            int userId,
            ClaimsPrincipal? session,
            ApplicationContext context,
            IConfiguration configuration)
        {
            BaseResponse response = new();

            try
            {
                var user = await context.AppUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return response.GenerateError(2007, "Kullanıcı bulunamadı.");

                var getUserResponse = new GetUserResponse
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Role = user.UserRole.ToString(),
                    ModTime = user.ModTime
                };

                // Profil resmi linki oluştur (varsa)
                if (!string.IsNullOrEmpty(user.UserImageLink))
                {
                    // Session varsa güvenli link oluştur, yoksa sadece dosya adını döndür
                    if (session != null)
                    {
                        string downloadServiceLink = configuration["AppSettings:FileSettings:DownloadServiceLink"] ?? "https://localhost:7132/api/file/download";
                        string secretKey = configuration["AppSettings:FileSettings:FileSecretKey"] ?? "ANIME_RANKER_FILE_SECRET_KEY_2024";
                        getUserResponse.UserImageLink = FileService.GenerateFileLinkWithSecret(
                            user.UserImageLink,
                            FileType.User,
                            session,
                            downloadServiceLink,
                            secretKey);
                    }
                    else
                    {
                        // Public erişim için basit link (güvenlik açısından daha sıkı kontrol gerekebilir)
                        getUserResponse.UserImageLink = user.UserImageLink;
                    }
                }
                else
                {
                    getUserResponse.UserImageLink = string.Empty;
                }

                response.Response = getUserResponse;
                if (session != null)
                {
                    response.SetUserID(SessionService.GetUserId(session));
                }
                return response.GenerateSuccess("Kullanıcı bilgileri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(2008, $"Kullanıcı bilgileri getirilirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Tüm kullanıcıları getir (pagination ile) - Admin için detaylı bilgiler
        /// </summary>
        public static async Task<BaseResponse> GetAllUsers(
            GetAllUsersRequest request,
            ClaimsPrincipal? session,
            ApplicationContext context,
            IConfiguration configuration)
        {
            BaseResponse response = new();

            try
            {
                // Admin kontrolü
                bool isAdmin = false;
                int? currentUserId = null;
                if (session != null)
                {
                    try
                    {
                        currentUserId = SessionService.GetUserId(session);
                        var currentUser = await context.AppUsers.FindAsync(currentUserId.Value);
                        isAdmin = currentUser != null &&
                                 (currentUser.UserRole == UserRole.Admin || currentUser.UserRole == UserRole.AdminAdmin);
                    }
                    catch { }
                }

                var query = context.AppUsers.AsNoTracking();

                // Arama sorgusu
                if (!string.IsNullOrWhiteSpace(request.SearchQuery))
                {
                    string searchLower = request.SearchQuery.ToLower();
                    query = query.Where(u => u.UserName.ToLower().Contains(searchLower));
                }

                // State filtresi
                if (request.IsActive.HasValue)
                {
                    query = query.Where(u => u.State == request.IsActive.Value);
                }

                // Toplam sayı
                int totalCount = await query.CountAsync();

                // Pagination
                var users = await query
                    .OrderBy(u => u.UserName)
                    .Skip((request.Page - 1) * request.Limit)
                    .Take(request.Limit)
                    .Select(u => new UserSummaryDto
                    {
                        Id = u.Id,
                        Username = u.UserName,
                        ModTime = u.ModTime
                    })
                    .ToListAsync();

                // Profil resmi linklerini oluştur
                string downloadServiceLink = configuration["AppSettings:FileSettings:DownloadServiceLink"] ?? "https://localhost:7132/api/file/download";
                string secretKey = configuration["AppSettings:FileSettings:FileSecretKey"] ?? "ANIME_RANKER_FILE_SECRET_KEY_2024";
                foreach (var user in users)
                {
                    var dbUser = await context.AppUsers.FindAsync(user.Id);
                    if (dbUser != null && !string.IsNullOrEmpty(dbUser.UserImageLink))
                    {
                        if (session != null)
                        {
                            user.UserImageLink = FileService.GenerateFileLinkWithSecret(
                                dbUser.UserImageLink,
                                FileType.User,
                                session,
                                downloadServiceLink,
                                secretKey);
                        }
                        else
                        {
                            user.UserImageLink = dbUser.UserImageLink;
                        }
                    }
                    else
                    {
                        user.UserImageLink = string.Empty;
                    }

                    // İstatistikler
                    user.TotalFollowers = await context.UserFollows.CountAsync(f => f.FollowingId == user.Id);
                }

                var userListResponse = new UserListResponse
                {
                    Users = users,
                    TotalCount = totalCount,
                    Page = request.Page,
                    Limit = request.Limit,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.Limit)
                };

                response.Response = userListResponse;
                if (session != null)
                {
                    response.SetUserID(SessionService.GetUserId(session));
                }
                return response.GenerateSuccess("Kullanıcılar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(2009, $"Kullanıcılar getirilirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Kullanıcı ara
        /// </summary>
        public static async Task<BaseResponse> SearchUsers(
            SearchUsersRequest request,
            ClaimsPrincipal? session,
            ApplicationContext context,
            IConfiguration configuration)
        {
            BaseResponse response = new();

            try
            {
                string searchLower = request.Query.ToLower();
                var users = await context.AppUsers
                    .AsNoTracking()
                    .Where(u => u.UserName.ToLower().Contains(searchLower))
                    .OrderBy(u => u.UserName)
                    .Skip((request.Page - 1) * request.Limit)
                    .Take(request.Limit)
                    .Select(u => new UserSummaryDto
                    {
                        Id = u.Id,
                        Username = u.UserName,
                        ModTime = u.ModTime
                    })
                    .ToListAsync();

                int totalCount = await context.AppUsers
                    .CountAsync(u => u.UserName.ToLower().Contains(searchLower));

                // Profil resmi linklerini oluştur
                string downloadServiceLink = configuration["AppSettings:FileSettings:DownloadServiceLink"] ?? "https://localhost:7132/api/file/download";
                string secretKey = configuration["AppSettings:FileSettings:FileSecretKey"] ?? "ANIME_RANKER_FILE_SECRET_KEY_2024";
                foreach (var user in users)
                {
                    var dbUser = await context.AppUsers.FindAsync(user.Id);
                    if (dbUser != null && !string.IsNullOrEmpty(dbUser.UserImageLink))
                    {
                        if (session != null)
                        {
                            user.UserImageLink = FileService.GenerateFileLinkWithSecret(
                                dbUser.UserImageLink,
                                FileType.User,
                                session,
                                downloadServiceLink,
                                secretKey);
                        }
                        else
                        {
                            user.UserImageLink = dbUser.UserImageLink;
                        }
                    }
                    else
                    {
                        user.UserImageLink = string.Empty;
                    }

                    user.TotalFollowers = await context.UserFollows.CountAsync(f => f.FollowingId == user.Id);
                }

                var userListResponse = new UserListResponse
                {
                    Users = users,
                    TotalCount = totalCount,
                    Page = request.Page,
                    Limit = request.Limit,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.Limit)
                };

                response.Response = userListResponse;
                if (session != null)
                {
                    response.SetUserID(SessionService.GetUserId(session));
                }
                return response.GenerateSuccess("Arama sonuçları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(2010, $"Kullanıcı aranırken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Kullanıcı bilgilerini güncelle (Admin veya kendi hesabı)
        /// </summary>
        public static async Task<BaseResponse> UpdateUser(
            UpdateUserRequest request,
            ClaimsPrincipal session,
            ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int currentUserId = SessionService.GetUserId(session);
                var currentUser = await context.AppUsers.FindAsync(currentUserId);
                if (currentUser == null)
                    return response.GenerateError(2011, "Kullanıcı bulunamadı.");

                // Hangi kullanıcı güncellenecek?
                int targetUserId = request.TargetUserId ?? currentUserId;
                bool isAdmin = currentUser.UserRole == UserRole.Admin || currentUser.UserRole == UserRole.AdminAdmin;
                bool isOwnAccount = targetUserId == currentUserId;

                // Yetki kontrolü: Admin veya kendi hesabı
                if (!isAdmin && !isOwnAccount)
                    return response.GenerateError(2025, "Bu işlem için yetkiniz yok.");

                var targetUser = await context.AppUsers.FindAsync(targetUserId);
                if (targetUser == null)
                    return response.GenerateError(2011, "Güncellenecek kullanıcı bulunamadı.");

                // Admin kontrolü: Admin, AdminAdmin rolü atayamaz
                if (isAdmin && !isOwnAccount && request.Role.HasValue)
                {
                    if (currentUser.UserRole == UserRole.Admin &&
                        (request.Role.Value == UserRole.Admin || request.Role.Value == UserRole.AdminAdmin))
                        return response.GenerateError(2026, "Admin, Admin veya AdminAdmin rolü atayamaz.");
                }

                // Username güncelleme
                if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != targetUser.UserName)
                {
                    // Username kontrolü
                    if (await context.AppUsers.AnyAsync(u => u.UserName.ToLower() == request.Username.ToLower() && u.Id != targetUserId))
                        return response.GenerateError(2012, "Bu kullanıcı adı zaten alınmış.");

                    targetUser.UserName = request.Username.ToLower();
                }

                // Admin için: Rol ve durum güncelleme
                if (isAdmin && !isOwnAccount)
                {
                    if (request.Role.HasValue)
                        targetUser.UserRole = request.Role.Value;

                    if (request.State.HasValue)
                        targetUser.State = request.State.Value;
                }

                targetUser.ModTime = DateTime.Now;
                targetUser.ModUser = currentUserId;

                await context.SaveChangesAsync();

                response.SetUserID(currentUserId);
                return response.GenerateSuccess("Kullanıcı bilgileri başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(2013, $"Kullanıcı güncellenirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Şifre değiştir
        /// </summary>
        public static async Task<BaseResponse> ChangePassword(
            ChangePasswordRequest request,
            ClaimsPrincipal session,
            ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);
                var user = await context.AppUsers.FindAsync(userId);

                if (user == null)
                    return response.GenerateError(2014, "Kullanıcı bulunamadı.");

                // Mevcut şifre kontrolü
                if (!VerifyPasswordHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                    return response.GenerateError(2015, "Mevcut şifre hatalı.");

                // Yeni şifre hash'le
                CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.ModTime = DateTime.Now;
                user.ModUser = userId;

                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Şifre başarıyla değiştirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(2016, $"Şifre değiştirilirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Profil güncelle (kullanıcı adı)
        /// </summary>
        public static async Task<BaseResponse> UpdateProfile(
            UpdateProfileRequest request,
            ClaimsPrincipal session,
            ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);
                var user = await context.AppUsers.FindAsync(userId);

                if (user == null)
                    return response.GenerateError(2017, "Kullanıcı bulunamadı.");

                // Username güncelleme
                if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != user.UserName)
                {
                    if (await context.AppUsers.AnyAsync(u => u.UserName.ToLower() == request.Username.ToLower() && u.Id != userId))
                        return response.GenerateError(2018, "Bu kullanıcı adı zaten alınmış.");

                    user.UserName = request.Username.ToLower();
                }

                user.ModTime = DateTime.Now;
                user.ModUser = userId;

                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Profil başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(2019, $"Profil güncellenirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Şifre hash oluştur
        /// </summary>
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        /// <summary>
        /// Şifre doğrula
        /// </summary>
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
        /// Kullanıcı sil (soft delete veya hard delete)
        /// </summary>
        public static async Task<BaseResponse> DeleteUser(
            DeleteUserRequest request,
            ClaimsPrincipal session,
            ApplicationContext context,
            IConfiguration configuration)
        {
            BaseResponse response = new();

            try
            {
                int currentUserId = SessionService.GetUserId(session);
                var currentUser = await context.AppUsers.FindAsync(currentUserId);
                if (currentUser == null)
                    return response.GenerateError(2020, "Kullanıcı bulunamadı.");

                // Admin kontrolü veya kendi hesabını silme kontrolü
                bool isAdmin = currentUser.UserRole == UserRole.Admin;
                bool isOwnAccount = currentUserId == request.UserId;

                if (!isAdmin && !isOwnAccount)
                    return response.GenerateError(2021, "Bu işlem için yetkiniz yok.");

                var userToDelete = await context.AppUsers
                    .FirstOrDefaultAsync(u => u.Id == request.UserId);

                if (userToDelete == null)
                    return response.GenerateError(2022, "Silinecek kullanıcı bulunamadı.");

                // Kendi hesabını silerken şifre doğrulama
                if (isOwnAccount && !string.IsNullOrEmpty(request.Password))
                {
                    if (!VerifyPasswordHash(request.Password, userToDelete.PasswordHash, userToDelete.PasswordSalt))
                        return response.GenerateError(2023, "Şifre hatalı.");
                }

                if (request.HardDelete)
                {
                    // Hard delete - Kullanıcıyı ve tüm ilişkili verileri kalıcı olarak sil
                    // Cascade delete ile listeler, yorumlar, beğeniler otomatik silinecek

                    // Kullanıcı dosyalarını sil
                    if (!string.IsNullOrEmpty(userToDelete.UserImageLink))
                    {
                        string baseDirectory = configuration["AppSettings:FileSettings:BaseDirectory"] ?? "Files";
                        FileService.DeleteFile(userToDelete.UserImageLink, FileType.User, baseDirectory);
                    }

                    // UserFollow ilişkilerini sil (takipçi/takip edilen)
                    var follows = await context.UserFollows
                        .Where(f => f.FollowerId == request.UserId || f.FollowingId == request.UserId)
                        .ToListAsync();
                    context.UserFollows.RemoveRange(follows);

                    // Bildirimleri sil
                    var notifications = await context.Notifications
                        .Where(n => n.AppUserId == request.UserId)
                        .ToListAsync();
                    context.Notifications.RemoveRange(notifications);

                    // Kullanıcıyı sil
                    context.AppUsers.Remove(userToDelete);
                    await context.SaveChangesAsync();

                    response.SetUserID(currentUserId);
                    return response.GenerateSuccess("Kullanıcı kalıcı olarak silindi.");
                }
                else
                {
                    // Soft delete - Sadece State'i false yap
                    userToDelete.State = false;
                    userToDelete.ModTime = DateTime.Now;
                    userToDelete.ModUser = currentUserId;
                    await context.SaveChangesAsync();

                    response.SetUserID(currentUserId);
                    return response.GenerateSuccess("Kullanıcı hesabı devre dışı bırakıldı.");
                }
            }
            catch (Exception ex)
            {
                return response.GenerateError(2024, $"Kullanıcı silinirken hata: {ex.Message}");
            }
        }
    }
}

