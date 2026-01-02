using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KeremProject1backend.Operations
{
    public static class SocialOperations
    {
        public static async Task<BaseResponse> LikeList(int listId, ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var list = await context.AnimeLists
                    .Include(l => l.AppUser)
                    .FirstOrDefaultAsync(l => l.Id == listId);

                if (list == null)
                    return response.GenerateError(9001, "Liste bulunamadı.");

                if (!list.IsPublic)
                    return response.GenerateError(9002, "Bu liste beğenilemez.");

                // Zaten beğenilmiş mi kontrol et
                var existingLike = await context.ListLikes
                    .FirstOrDefaultAsync(l => l.AnimeListId == listId && l.AppUserId == userId);

                if (existingLike != null)
                {
                    // Beğeniyi kaldır
                    context.ListLikes.Remove(existingLike);
                    list.LikeCount = Math.Max(0, list.LikeCount - 1);
                    await context.SaveChangesAsync();

                    response.SetUserID(userId);
                    return response.GenerateSuccess("Beğeni kaldırıldı.");
                }

                // Beğeni ekle
                var like = new ListLike
                {
                    AnimeListId = listId,
                    AppUserId = userId,
                    CreatedAt = DateTime.Now
                };

                await context.ListLikes.AddAsync(like);
                list.LikeCount++;

                // Bildirim oluştur (liste sahibine)
                if (list.AppUserId != userId)
                {
                    var notification = new Notification
                    {
                        AppUserId = list.AppUserId,
                        Type = "like",
                        Message = $"{session.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Bir kullanıcı"} listenizi beğendi.",
                        RelatedListId = list.Id,
                        RelatedUserId = userId,
                        CreatedAt = DateTime.Now
                    };
                    await context.Notifications.AddAsync(notification);
                }

                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Liste beğenildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9003, $"Beğeni hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> FollowUser(FollowUserRequest request, ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int followerId = SessionService.GetUserId(session);
                int followingId = request.UserId;

                if (followerId == followingId)
                    return response.GenerateError(9004, "Kendinizi takip edemezsiniz.");

                var targetUser = await context.AppUsers.FindAsync(followingId);
                if (targetUser == null)
                    return response.GenerateError(9005, "Kullanıcı bulunamadı.");

                // Zaten takip ediliyor mu kontrol et
                var existingFollow = await context.UserFollows
                    .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

                if (existingFollow != null)
                {
                    // Takibi kaldır
                    context.UserFollows.Remove(existingFollow);
                    await context.SaveChangesAsync();

                    response.SetUserID(followerId);
                    return response.GenerateSuccess("Takip kaldırıldı.");
                }

                // Takip et
                var follow = new UserFollow
                {
                    FollowerId = followerId,
                    FollowingId = followingId,
                    CreatedAt = DateTime.Now
                };

                await context.UserFollows.AddAsync(follow);

                // Bildirim oluştur
                var notification = new Notification
                {
                    AppUserId = followingId,
                    Type = "follow",
                    Message = $"{session.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Bir kullanıcı"} sizi takip etmeye başladı.",
                    RelatedUserId = followerId,
                    CreatedAt = DateTime.Now
                };
                await context.Notifications.AddAsync(notification);

                await context.SaveChangesAsync();

                response.SetUserID(followerId);
                return response.GenerateSuccess("Kullanıcı takip edildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9006, $"Takip hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> GetUserProfile(int userId, ApplicationContext context, IConfiguration configuration, ClaimsPrincipal? session = null)
        {
            BaseResponse response = new();

            try
            {
                int? currentUserId = null;
                if (session != null)
                {
                    try
                    {
                        currentUserId = SessionService.GetUserId(session);
                    }
                    catch { }
                }

                var user = await context.AppUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return response.GenerateError(9007, "Kullanıcı bulunamadı.");

                var isFollowing = false;
                if (currentUserId.HasValue && currentUserId.Value != userId)
                {
                    isFollowing = await context.UserFollows
                        .AnyAsync(f => f.FollowerId == currentUserId.Value && f.FollowingId == userId);
                }

                // Profil resmi linki oluştur
                string? userImageLink = null;
                if (!string.IsNullOrEmpty(user.UserImageLink))
                {
                    if (session != null)
                    {
                        string downloadServiceLink = configuration["AppSettings:FileSettings:DownloadServiceLink"] ?? "https://localhost:7132/api/file/download";
                        userImageLink = FileService.GenerateFileLink(
                            user.UserImageLink,
                            FileType.User,
                            session,
                            downloadServiceLink);
                    }
                    else
                    {
                        // Public erişim için basit link
                        userImageLink = user.UserImageLink;
                    }
                }

                var profile = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    UserImageLink = userImageLink,
                    MalUsername = user.MalUsername,
                    TotalLists = await context.AnimeLists.CountAsync(l => l.AppUserId == userId),
                    TotalFollowers = await context.UserFollows.CountAsync(f => f.FollowingId == userId),
                    TotalFollowing = await context.UserFollows.CountAsync(f => f.FollowerId == userId),
                    IsFollowing = isFollowing,
                    IsOwnProfile = currentUserId.HasValue && currentUserId.Value == userId
                };

                response.Response = profile;
                return response.GenerateSuccess("Profil başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9008, $"Profil getirme hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> GetNotifications(ClaimsPrincipal session, ApplicationContext context, int page = 1, int limit = 20)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var notifications = await context.Notifications
                    .AsNoTracking()
                    .Where(n => n.AppUserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        Type = n.Type,
                        Message = n.Message,
                        RelatedListId = n.RelatedListId,
                        RelatedUserId = n.RelatedUserId,
                        RelatedUsername = n.RelatedUserId.HasValue
                            ? context.AppUsers.Where(u => u.Id == n.RelatedUserId.Value).Select(u => u.UserName).FirstOrDefault() ?? string.Empty
                            : string.Empty,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt
                    })
                    .ToListAsync();

                var unreadCount = await context.Notifications
                    .CountAsync(n => n.AppUserId == userId && !n.IsRead);

                response.Response = new
                {
                    Notifications = notifications,
                    UnreadCount = unreadCount,
                    Page = page,
                    TotalPages = (int)Math.Ceiling(await context.Notifications.CountAsync(n => n.AppUserId == userId) / (double)limit)
                };

                response.SetUserID(userId);
                return response.GenerateSuccess("Bildirimler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9009, $"Bildirim getirme hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> MarkNotificationAsRead(int notificationId, ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var notification = await context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.AppUserId == userId);

                if (notification == null)
                    return response.GenerateError(9010, "Bildirim bulunamadı.");

                notification.IsRead = true;
                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Bildirim okundu olarak işaretlendi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9011, $"Bildirim güncelleme hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> MarkAllNotificationsAsRead(ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                await context.Notifications
                    .Where(n => n.AppUserId == userId && !n.IsRead)
                    .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));

                response.SetUserID(userId);
                return response.GenerateSuccess("Tüm bildirimler okundu olarak işaretlendi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9012, $"Bildirim güncelleme hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> CreateTemplate(CreateTemplateRequest request, ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var sourceList = await context.AnimeLists
                    .Include(l => l.Tiers)
                    .ThenInclude(t => t.Items)
                    .FirstOrDefaultAsync(l => l.Id == request.ListId && l.AppUserId == userId);

                if (sourceList == null)
                    return response.GenerateError(9013, "Liste bulunamadı veya bu liste üzerinde yetkiniz yok.");

                // Yeni liste oluştur (şablon olarak)
                var templateList = new AnimeList
                {
                    Title = request.TemplateName,
                    Mode = sourceList.Mode,
                    AppUserId = userId,
                    IsTemplate = true,
                    IsPublic = true,
                    CreatedAt = DateTime.Now,
                    ModTime = DateTime.Now
                };

                // Tier'ları kopyala
                foreach (var tier in sourceList.Tiers.OrderBy(t => t.Order))
                {
                    var newTier = new Tier
                    {
                        Title = tier.Title,
                        Color = tier.Color,
                        Order = tier.Order,
                        Items = tier.Items.Select(item => new RankedItem
                        {
                            AnimeMalId = item.AnimeMalId,
                            RankInTier = item.RankInTier
                        }).ToList()
                    };
                    templateList.Tiers.Add(newTier);
                }

                await context.AnimeLists.AddAsync(templateList);
                await context.SaveChangesAsync();

                response.Response = new { TemplateId = templateList.Id };
                response.SetUserID(userId);
                return response.GenerateSuccess("Şablon başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9014, $"Şablon oluşturma hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> GetTemplates(ApplicationContext context, int page = 1, int limit = 20)
        {
            BaseResponse response = new();

            try
            {
                var templates = await context.AnimeLists
                    .AsNoTracking()
                    .Include(l => l.AppUser)
                    .Where(l => l.IsTemplate && l.IsPublic)
                    .OrderByDescending(l => l.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(l => new TemplateDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Mode = l.Mode.ToString(),
                        AuthorUsername = l.AppUser.UserName,
                        UseCount = 0, // TODO: Use count tracking eklenebilir
                        CreatedAt = l.CreatedAt
                    })
                    .ToListAsync();

                response.Response = templates;
                return response.GenerateSuccess("Şablonlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9015, $"Şablon getirme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Bildirim sil
        /// </summary>
        public static async Task<BaseResponse> DeleteNotification(
            int notificationId,
            ClaimsPrincipal session,
            ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var notification = await context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.AppUserId == userId);

                if (notification == null)
                    return response.GenerateError(9016, "Bildirim bulunamadı veya bu bildirim üzerinde yetkiniz yok.");

                context.Notifications.Remove(notification);
                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Bildirim başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9017, $"Bildirim silme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Tüm bildirimleri sil
        /// </summary>
        public static async Task<BaseResponse> DeleteAllNotifications(
            ClaimsPrincipal session,
            ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var notifications = await context.Notifications
                    .Where(n => n.AppUserId == userId)
                    .ToListAsync();

                context.Notifications.RemoveRange(notifications);
                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess($"{notifications.Count} bildirim başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9018, $"Bildirimler silinirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Şablon sil
        /// </summary>
        public static async Task<BaseResponse> DeleteTemplate(
            int templateId,
            ClaimsPrincipal session,
            ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var template = await context.AnimeLists
                    .FirstOrDefaultAsync(l => l.Id == templateId && l.AppUserId == userId && l.IsTemplate);

                if (template == null)
                    return response.GenerateError(9019, "Şablon bulunamadı veya bu şablon üzerinde yetkiniz yok.");

                context.AnimeLists.Remove(template);
                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Şablon başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(9020, $"Şablon silme hatası: {ex.Message}");
            }
        }
    }
}

