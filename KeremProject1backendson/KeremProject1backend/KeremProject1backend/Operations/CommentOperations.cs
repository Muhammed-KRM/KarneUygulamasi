using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Requests;
using KeremProject1backend.Models.Responses;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KeremProject1backend.Operations
{
    public static class CommentOperations
    {
        public static async Task<BaseResponse> AddComment(AddCommentRequest request, ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var list = await context.AnimeLists
                    .Include(l => l.AppUser)
                    .FirstOrDefaultAsync(l => l.Id == request.ListId);

                if (list == null)
                    return response.GenerateError(8001, "Liste bulunamadı.");

                // Sadece public listelere yorum yapılabilir
                if (!list.IsPublic)
                    return response.GenerateError(8002, "Bu liste yorumlara kapalı.");

                var comment = new ListComment
                {
                    AnimeListId = request.ListId,
                    AppUserId = userId,
                    Content = request.Content,
                    CreatedAt = DateTime.Now
                };

                await context.ListComments.AddAsync(comment);

                // Bildirim oluştur (liste sahibine)
                if (list.AppUserId != userId)
                {
                    var notification = new Notification
                    {
                        AppUserId = list.AppUserId,
                        Type = "comment",
                        Message = $"{session.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Bir kullanıcı"} listenize yorum yaptı.",
                        RelatedListId = list.Id,
                        RelatedUserId = userId,
                        CreatedAt = DateTime.Now
                    };
                    await context.Notifications.AddAsync(notification);
                }

                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Yorum başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(8003, $"Yorum ekleme hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> GetComments(int listId, ApplicationContext context, ClaimsPrincipal? session = null)
        {
            BaseResponse response = new();

            try
            {
                int? userId = null;
                if (session != null)
                {
                    try
                    {
                        userId = SessionService.GetUserId(session);
                    }
                    catch { }
                }

                var comments = await context.ListComments
                    .AsNoTracking()
                    .Include(c => c.AppUser)
                    .Where(c => c.AnimeListId == listId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        ListId = c.AnimeListId,
                        UserId = c.AppUserId,
                        Username = c.AppUser.UserName,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        ModTime = c.ModTime,
                        IsOwnComment = userId.HasValue && c.AppUserId == userId.Value
                    })
                    .ToListAsync();

                response.Response = comments;
                return response.GenerateSuccess("Yorumlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(8004, $"Yorum getirme hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> UpdateComment(UpdateCommentRequest request, ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var comment = await context.ListComments
                    .FirstOrDefaultAsync(c => c.Id == request.CommentId && c.AppUserId == userId);

                if (comment == null)
                    return response.GenerateError(8005, "Yorum bulunamadı veya bu yorum üzerinde yetkiniz yok.");

                comment.Content = request.Content;
                comment.ModTime = DateTime.Now;
                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Yorum başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(8006, $"Yorum güncelleme hatası: {ex.Message}");
            }
        }

        public static async Task<BaseResponse> DeleteComment(int commentId, ClaimsPrincipal session, ApplicationContext context)
        {
            BaseResponse response = new();

            try
            {
                int userId = SessionService.GetUserId(session);

                var comment = await context.ListComments
                    .FirstOrDefaultAsync(c => c.Id == commentId && c.AppUserId == userId);

                if (comment == null)
                    return response.GenerateError(8007, "Yorum bulunamadı veya bu yorum üzerinde yetkiniz yok.");

                context.ListComments.Remove(comment);
                await context.SaveChangesAsync();

                response.SetUserID(userId);
                return response.GenerateSuccess("Yorum başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return response.GenerateError(8008, $"Yorum silme hatası: {ex.Message}");
            }
        }
    }
}

