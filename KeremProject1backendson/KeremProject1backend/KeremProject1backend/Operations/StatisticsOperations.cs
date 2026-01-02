using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KeremProject1backend.Operations
{
    public static class StatisticsOperations
    {
        public static async Task<BaseResponse> GetUserStatistics(int userId, ApplicationContext context, IHttpClientFactory httpClientFactory, IConfiguration config, ClaimsPrincipal? session = null)
        {
            BaseResponse response = new();

            try
            {
                var user = await context.AppUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return response.GenerateError(7001, "Kullanıcı bulunamadı.");

                var stats = new UserStatisticsDto
                {
                    TotalLists = await context.AnimeLists.CountAsync(l => l.AppUserId == userId),
                    PublicLists = await context.AnimeLists.CountAsync(l => l.AppUserId == userId && l.IsPublic),
                    TotalLikes = await context.ListLikes.CountAsync(l => l.AnimeList.AppUserId == userId),
                    TotalFollowers = await context.UserFollows.CountAsync(f => f.FollowingId == userId),
                    TotalFollowing = await context.UserFollows.CountAsync(f => f.FollowerId == userId)
                };

                // MAL'dan anime istatistikleri çek
                if (!string.IsNullOrEmpty(user.MalAccessToken))
                {
                    try
                    {
                        var malListResponse = await MalIntegrationOperations.GetMyAnimeList(session!, context, httpClientFactory, config);
                        if (!malListResponse.Errored && malListResponse.Response != null)
                        {
                            var malList = malListResponse.Response as MalAnimeListResponse;
                            if (malList?.Data != null)
                            {
                                var completedAnimes = malList.Data.Where(d => d.ListStatus.Status == "completed").ToList();
                                stats.TotalAnimeWatched = completedAnimes.Count;

                                var scoredAnimes = completedAnimes.Where(a => a.ListStatus.Score > 0).ToList();
                                if (scoredAnimes.Any())
                                {
                                    stats.AverageScore = scoredAnimes.Average(a => a.ListStatus.Score);

                                    // Score distribution
                                    for (int i = 1; i <= 10; i++)
                                    {
                                        stats.ScoreDistribution[i] = scoredAnimes.Count(a => a.ListStatus.Score == i);
                                    }
                                }

                                // Year ve Genre distribution için Jikan'dan detay çek (sınırlı sayıda)
                                var yearCounts = new Dictionary<int, int>();
                                var genreCounts = new Dictionary<string, int>();

                                int processed = 0;
                                foreach (var animeNode in completedAnimes.Take(50)) // İlk 50 anime için
                                {
                                    var details = await JikanService.GetAnimeDetails(animeNode.Node.Id, httpClientFactory);
                                    if (details != null)
                                    {
                                        if (details.Year.HasValue)
                                        {
                                            yearCounts[details.Year.Value] = yearCounts.GetValueOrDefault(details.Year.Value, 0) + 1;
                                        }

                                        foreach (var genre in details.Genres)
                                        {
                                            genreCounts[genre.Name] = genreCounts.GetValueOrDefault(genre.Name, 0) + 1;
                                        }
                                    }

                                    processed++;
                                    if (processed % 10 == 0)
                                        await Task.Delay(1000); // Rate limiting
                                }

                                stats.YearDistribution = yearCounts;
                                stats.GenreDistribution = genreCounts;
                            }
                        }
                    }
                    catch { } // MAL bağlantısı yoksa devam et
                }

                response.Response = stats;
                return response.GenerateSuccess("İstatistikler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(7002, $"İstatistik getirme hatası: {ex.Message}");
            }
        }
    }
}

