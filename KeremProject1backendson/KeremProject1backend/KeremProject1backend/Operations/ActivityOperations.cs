using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KeremProject1backend.Operations
{
    public static class ActivityOperations
    {
        public static async Task<BaseResponse> GetUserActivity(int userId, ApplicationContext context, int page = 1, int limit = 20, ClaimsPrincipal? currentUser = null)
        {
            BaseResponse response = new();

            try
            {
                var activities = new List<ActivityDto>();

                // Liste oluşturma aktiviteleri
                var createdLists = await context.AnimeLists
                    .AsNoTracking()
                    .Where(l => l.AppUserId == userId)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(limit)
                    .Select(l => new ActivityDto
                    {
                        Id = l.Id,
                        Type = "list_created",
                        Description = $"'{l.Title}' listesi oluşturuldu",
                        RelatedListId = l.Id,
                        RelatedListTitle = l.Title,
                        CreatedAt = l.CreatedAt
                    })
                    .ToListAsync();

                activities.AddRange(createdLists);

                // Liste güncelleme aktiviteleri
                var updatedLists = await context.AnimeLists
                    .AsNoTracking()
                    .Where(l => l.AppUserId == userId && l.ModTime != l.CreatedAt)
                    .OrderByDescending(l => l.ModTime)
                    .Take(limit)
                    .Select(l => new ActivityDto
                    {
                        Id = l.Id,
                        Type = "list_updated",
                        Description = $"'{l.Title}' listesi güncellendi",
                        RelatedListId = l.Id,
                        RelatedListTitle = l.Title,
                        CreatedAt = l.ModTime
                    })
                    .ToListAsync();

                activities.AddRange(updatedLists);

                // Yorum aktiviteleri
                var comments = await context.ListComments
                    .AsNoTracking()
                    .Include(c => c.AnimeList)
                    .Where(c => c.AppUserId == userId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(limit)
                    .Select(c => new ActivityDto
                    {
                        Id = c.Id,
                        Type = "comment_added",
                        Description = $"'{c.AnimeList.Title}' listesine yorum yapıldı",
                        RelatedListId = c.AnimeListId,
                        RelatedListTitle = c.AnimeList.Title,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                activities.AddRange(comments);

                // Beğeni aktiviteleri
                var likes = await context.ListLikes
                    .AsNoTracking()
                    .Include(l => l.AnimeList)
                    .Where(l => l.AppUserId == userId)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(limit)
                    .Select(l => new ActivityDto
                    {
                        Id = l.Id,
                        Type = "list_liked",
                        Description = $"'{l.AnimeList.Title}' listesi beğenildi",
                        RelatedListId = l.AnimeListId,
                        RelatedListTitle = l.AnimeList.Title,
                        CreatedAt = l.CreatedAt
                    })
                    .ToListAsync();

                activities.AddRange(likes);

                // Takip aktiviteleri
                var follows = await context.UserFollows
                    .AsNoTracking()
                    .Include(f => f.Following)
                    .Where(f => f.FollowerId == userId)
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(limit)
                    .Select(f => new ActivityDto
                    {
                        Id = f.Id,
                        Type = "user_followed",
                        Description = $"'{f.Following.UserName}' kullanıcısı takip edildi",
                        RelatedUserId = f.FollowingId,
                        RelatedUsername = f.Following.UserName,
                        CreatedAt = f.CreatedAt
                    })
                    .ToListAsync();

                activities.AddRange(follows);

                // Tarihe göre sırala ve sayfalama
                var sortedActivities = activities
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToList();

                var totalCount = activities.Count;

                response.Response = new
                {
                    Activities = sortedActivities,
                    TotalCount = totalCount,
                    Page = page,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
                };

                return response.GenerateSuccess("Aktivite geçmişi başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(12001, $"Aktivite getirme hatası: {ex.Message}");
            }
        }
    }
}

