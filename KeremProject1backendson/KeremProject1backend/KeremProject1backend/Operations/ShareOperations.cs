using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;

namespace KeremProject1backend.Operations
{
    public static class ShareOperations
    {
        private static string GenerateShareToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("=", "").Replace("+", "-").Replace("/", "_");
        }

        public static async Task<BaseResponse> SetListVisibility(SetListVisibilityRequest request, ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var list = await context.AnimeLists
                    .FirstOrDefaultAsync(l => l.Id == request.ListId && l.AppUserId == userId);

                if (list == null)
                    return response.GenerateError(5001, "Liste bulunamadı veya bu liste üzerinde yetkiniz yok.");

                list.IsPublic = request.IsPublic;
                
                // Eğer public yapılıyorsa ve token yoksa oluştur
                if (request.IsPublic && string.IsNullOrEmpty(list.ShareToken))
                {
                    list.ShareToken = GenerateShareToken();
                }
                // Eğer private yapılıyorsa token'ı temizle
                else if (!request.IsPublic)
                {
                    list.ShareToken = null;
                }

                list.ModTime = DateTime.Now;
                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Liste görünürlüğü başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(5002, $"Görünürlük güncelleme hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> GenerateShareLink(GenerateShareLinkRequest request, ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var list = await context.AnimeLists
                    .FirstOrDefaultAsync(l => l.Id == request.ListId && l.AppUserId == userId);

                if (list == null)
                    return response.GenerateError(5003, "Liste bulunamadı veya bu liste üzerinde yetkiniz yok.");

                // Token yoksa oluştur
                if (string.IsNullOrEmpty(list.ShareToken))
                {
                    list.ShareToken = GenerateShareToken();
                    list.IsPublic = true;
                    await context.SaveChangesAsync();
                }

                var shareUrl = $"https://yourdomain.com/share/{list.ShareToken}";
                response.Response = new ShareLinkResponse
                {
                    ShareUrl = shareUrl,
                    ShareToken = list.ShareToken
                };

                response.SetUserID(userId);
                return response.GenerateSuccess("Paylaşım linki oluşturuldu.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(5004, $"Paylaşım linki oluşturma hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> GetPublicListByToken(string shareToken, ApplicationContext context, IHttpClientFactory httpClientFactory, ClaimsPrincipal? session = null, IConfiguration? config = null)
        {
            BaseResponse response = new();

            try
            {
                var list = await context.AnimeLists
                    .AsNoTracking()
                    .Include(l => l.AppUser)
                    .Include(l => l.Tiers.OrderBy(t => t.Order))
                    .ThenInclude(t => t.Items.OrderBy(i => i.RankInTier))
                    .FirstOrDefaultAsync(l => l.ShareToken == shareToken && l.IsPublic);

                if (list == null)
                    return response.GenerateError(5005, "Liste bulunamadı veya paylaşıma kapalı.");

                // View count'u artır
                list.ViewCount++;
                context.Entry(list).Property(x => x.ViewCount).IsModified = true;
                await context.SaveChangesAsync();

                // Kullanıcı giriş yapmışsa beğenip beğenmediğini kontrol et
                bool isLiked = false;
                if (session != null)
                {
                    int? userId = null;
                    try
                    {
                        userId = SessionService.GetUserId(session);
                        isLiked = await context.ListLikes
                            .AnyAsync(l => l.AnimeListId == list.Id && l.AppUserId == userId);
                    }
                    catch { }
                }

                // Tüm anime ID'lerini topla (optimizasyon için toplu çekme)
                var allAnimeIds = list.Tiers
                    .SelectMany(t => t.Items)
                    .Select(i => i.AnimeMalId)
                    .Distinct()
                    .ToList();

                // Anime detaylarını toplu olarak cache'den veya API'den çek
                var animeDetails = await AnimeCacheService.GetAnimeDetailsBatch(
                    allAnimeIds,
                    context,
                    httpClientFactory,
                    config);

                // Liste detaylarını cache'den al
                var listDto = new AnimeListDto
                {
                    Id = list.Id,
                    Title = list.Title,
                    Mode = list.Mode.ToString(),
                    Tiers = new List<TierDto>()
                };

                foreach (var tier in list.Tiers.OrderBy(t => t.Order))
                {
                    var tierDto = new TierDto
                    {
                        Id = tier.Id,
                        Title = tier.Title,
                        Color = tier.Color,
                        Order = tier.Order,
                        Items = new List<RankedItemDto>()
                    };

                    foreach (var item in tier.Items.OrderBy(i => i.RankInTier))
                    {
                        var details = animeDetails.GetValueOrDefault(item.AnimeMalId, 
                            (Title: $"Anime {item.AnimeMalId}", ImageUrl: string.Empty));
                        
                        tierDto.Items.Add(new RankedItemDto
                        {
                            Id = item.Id,
                            AnimeMalId = item.AnimeMalId,
                            RankInTier = item.RankInTier,
                            Title = details.Title,
                            ImageUrl = details.ImageUrl
                        });
                    }

                    listDto.Tiers.Add(tierDto);
                }

                // Public list bilgileri
                var publicListInfo = new PublicListDto
                {
                    Id = list.Id,
                    Title = list.Title,
                    Mode = list.Mode.ToString(),
                    AuthorUsername = list.AppUser.UserName,
                    AuthorId = list.AppUserId,
                    ViewCount = list.ViewCount,
                    LikeCount = list.LikeCount,
                    CreatedAt = list.CreatedAt,
                    IsLiked = isLiked
                };

                response.Response = new
                {
                    List = listDto,
                    PublicInfo = publicListInfo
                };

                return response.GenerateSuccess("Liste başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(5006, $"Liste getirme hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> GetPublicLists(int page = 1, int limit = 20, ApplicationContext context = null!, ClaimsPrincipal? session = null, IConfiguration? configuration = null)
        {
            BaseResponse response = new();

            try
            {
                var lists = await context.AnimeLists
                    .AsNoTracking()
                    .Include(l => l.AppUser)
                    .Where(l => l.IsPublic)
                    .OrderByDescending(l => l.LikeCount)
                    .ThenByDescending(l => l.ViewCount)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(l => new PublicListDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Mode = l.Mode.ToString(),
                        AuthorUsername = l.AppUser.UserName,
                        AuthorId = l.AppUserId,
                        ViewCount = l.ViewCount,
                        LikeCount = l.LikeCount,
                        CreatedAt = l.CreatedAt,
                        IsLiked = false,
                        ListImageLink = l.ListImageLink
                    })
                    .ToListAsync();

                var totalCount = await context.AnimeLists.CountAsync(l => l.IsPublic);

                // Görsel linklerini oluştur (session varsa)
                if (session != null && configuration != null)
                {
                    string downloadServiceLink = configuration["AppSettings:FileSettings:DownloadServiceLink"] ?? "https://localhost:7132/api/file/download";
                    string secretKey = configuration["AppSettings:FileSettings:FileSecretKey"] ?? "ANIME_RANKER_FILE_SECRET_KEY_2024";

                    foreach (var list in lists)
                    {
                        if (!string.IsNullOrEmpty(list.ListImageLink))
                        {
                            list.ListImageLink = FileService.GenerateFileLinkWithSecret(list.ListImageLink, FileType.List, session, downloadServiceLink, secretKey);
                        }
                    }
                }
                else
                {
                    // Session yoksa görsel linklerini null yap
                    foreach (var list in lists)
                    {
                        list.ListImageLink = null;
                    }
                }

                response.Response = new
                {
                    Lists = lists,
                    TotalCount = totalCount,
                    Page = page,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
                };

                return response.GenerateSuccess("Public listeler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(5007, $"Liste getirme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Paylaşım linkini sil (ShareToken'ı temizle)
        /// </summary>
        public static async Task<BaseResponse> DeleteShareLink(
            int listId,
            ClaimsPrincipal session,
            ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var list = await context.AnimeLists
                    .FirstOrDefaultAsync(l => l.Id == listId && l.AppUserId == userId);

                if (list == null)
                    return response.GenerateError(5008, "Liste bulunamadı veya bu liste üzerinde yetkiniz yok.");

                // ShareToken'ı temizle ve public'i false yap
                list.ShareToken = null;
                list.IsPublic = false;
                list.ModTime = DateTime.Now;
                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Paylaşım linki başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(5009, $"Paylaşım linki silme hatası: {ex.Message}");
            }
        }
    }
}

