using Hangfire;
using KeremProject1backend.Core.Constants;
using KeremProject1backend.Hubs;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Models.DTOs.Responses;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KeremProject1backend.Operations;

public class SocialOperations
{
    private readonly ApplicationContext _context;
    private readonly SessionService _sessionService;
    private readonly CacheService _cacheService;
    private readonly AuditService _auditService;
    private readonly NotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly AuthorizationService _authorizationService;

    public SocialOperations(
        ApplicationContext context,
        SessionService sessionService,
        CacheService cacheService,
        AuditService auditService,
        NotificationService notificationService,
        IHubContext<NotificationHub> notificationHub,
        AuthorizationService authorizationService)
    {
        _context = context;
        _sessionService = sessionService;
        _cacheService = cacheService;
        _auditService = auditService;
        _notificationService = notificationService;
        _notificationHub = notificationHub;
        _authorizationService = authorizationService;
    }

    // ========== CONTENT MANAGEMENT ==========

    public async Task<BaseResponse<ContentDto>> CreateContentAsync(CreateContentRequest request)
    {
        var userId = _sessionService.GetUserId();

        // Validation
        if (string.IsNullOrWhiteSpace(request.Title))
            return BaseResponse<ContentDto>.ErrorResponse("Title is required", ErrorCodes.ValidationFailed);

        if (request.LessonId.HasValue && !await _context.Lessons.AnyAsync(l => l.Id == request.LessonId))
            return BaseResponse<ContentDto>.ErrorResponse("Lesson not found", ErrorCodes.ValidationFailed);

        if (request.TopicId.HasValue && !await _context.Topics.AnyAsync(t => t.Id == request.TopicId))
            return BaseResponse<ContentDto>.ErrorResponse("Topic not found", ErrorCodes.ValidationFailed);

        var content = new Content
        {
            AuthorId = userId,
            ContentType = request.ContentType,
            Title = request.Title,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            VideoUrl = request.VideoUrl,
            FileUrl = request.FileUrl,
            LessonId = request.LessonId,
            TopicId = request.TopicId,
            Difficulty = request.Difficulty,
            TagsJson = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync("Content:*");
        await _cacheService.RemoveByPatternAsync("Feed:*");
        await _cacheService.RemoveByPatternAsync("Trending:*");
        await _cacheService.RemoveByPatternAsync("Popular:*");

        // SignalR: Takipçilere bildirim
        var followers = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowingId == userId)
            .Select(f => f.FollowerId)
            .ToListAsync();

        foreach (var followerId in followers)
        {
            await _notificationHub.Clients.Group($"User_{followerId}")
                .SendAsync("NewContent", new { AuthorId = userId, ContentId = content.Id, Title = content.Title });
        }

        // Hangfire: RediSearch index (eğer aktifse)
        BackgroundJob.Enqueue(() => IndexContentAsync(content.Id));

        await _auditService.LogAsync(userId, "ContentCreated", 
            JsonSerializer.Serialize(new { ContentId = content.Id, Type = request.ContentType }));

        var dto = await MapToContentDtoAsync(content, userId);
        return BaseResponse<ContentDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<ContentDetailDto>> GetContentByIdAsync(int contentId, bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"Content:{contentId}:Detail:User:{userId}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<ContentDetailDto>(cacheKey);
            if (cached != null)
                return BaseResponse<ContentDetailDto>.SuccessResponse(cached);
        }

        var content = await _context.Contents
            .AsNoTracking()
            .Where(c => c.Id == contentId && !c.IsDeleted)
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .Include(c => c.Comments.Where(com => !com.IsDeleted).OrderByDescending(com => com.CreatedAt).Take(10))
                .ThenInclude(com => com.Author)
            .FirstOrDefaultAsync();

        if (content == null)
            return BaseResponse<ContentDetailDto>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        // View tracking (background job ile yapılabilir)
        var existingView = await _context.Interactions
            .FirstOrDefaultAsync(i => i.ContentId == contentId && i.UserId == userId && i.Type == InteractionType.View);

        if (existingView == null)
        {
            var view = new Interaction
            {
                UserId = userId,
                ContentId = contentId,
                Type = InteractionType.View,
                CreatedAt = DateTime.UtcNow
            };
            _context.Interactions.Add(view);

            // Views count'u güncelle
            content.ViewsCount++;
            await _context.SaveChangesAsync();

            // Cache invalidation
            await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");
        }

        var dto = await MapToContentDetailDtoAsync(content, userId);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15));

        return BaseResponse<ContentDetailDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<bool>> UpdateContentAsync(int contentId, UpdateContentRequest request)
    {
        // 1. YETKİ KONTROLÜ - Herkes yapabilir (içerik sahibi kontrolü iş mantığında)
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.AdminAdmin,
            UserRole.Admin,
            UserRole.Manager,
            UserRole.Teacher,
            UserRole.StandaloneTeacher,
            UserRole.Student,
            UserRole.StandaloneStudent);
        if (authError != null)
            return BaseResponse<bool>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted);

        if (content == null)
            return BaseResponse<bool>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        // İçerik sahibi kontrolü
        if (content.AuthorId != userId)
        {
            // Admin içerik güncelleyebilir
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || (user.GlobalRole != UserRole.AdminAdmin && user.GlobalRole != UserRole.Admin))
                return BaseResponse<bool>.ErrorResponse("Bu içeriği güncelleme yetkiniz yok", ErrorCodes.AccessDenied);
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Title))
            content.Title = request.Title;

        if (request.Description != null)
            content.Description = request.Description;

        if (request.ImageUrl != null)
            content.ImageUrl = request.ImageUrl;

        if (request.VideoUrl != null)
            content.VideoUrl = request.VideoUrl;

        if (request.FileUrl != null)
            content.FileUrl = request.FileUrl;

        if (request.LessonId.HasValue)
            content.LessonId = request.LessonId;

        if (request.TopicId.HasValue)
            content.TopicId = request.TopicId;

        if (request.Difficulty.HasValue)
            content.Difficulty = request.Difficulty;

        if (request.Tags != null)
            content.TagsJson = JsonSerializer.Serialize(request.Tags);

        content.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");
        await _cacheService.RemoveByPatternAsync("Feed:*");
        await _cacheService.RemoveByPatternAsync("Trending:*");
        await _cacheService.RemoveByPatternAsync("Popular:*");

        // Hangfire: RediSearch index güncelleme
        BackgroundJob.Enqueue(() => IndexContentAsync(contentId));

        await _auditService.LogAsync(userId, "ContentUpdated", 
            JsonSerializer.Serialize(new { ContentId = contentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> DeleteContentAsync(int contentId)
    {
        // 1. YETKİ KONTROLÜ - Herkes yapabilir (içerik sahibi kontrolü iş mantığında)
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.AdminAdmin,
            UserRole.Admin,
            UserRole.Manager,
            UserRole.Teacher,
            UserRole.StandaloneTeacher,
            UserRole.Student,
            UserRole.StandaloneStudent);
        if (authError != null)
            return BaseResponse<bool>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted);

        if (content == null)
            return BaseResponse<bool>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        // İçerik sahibi kontrolü
        if (content.AuthorId != userId)
        {
            // Admin içerik silebilir
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || (user.GlobalRole != UserRole.AdminAdmin && user.GlobalRole != UserRole.Admin))
                return BaseResponse<bool>.ErrorResponse("Bu içeriği silme yetkiniz yok", ErrorCodes.AccessDenied);
        }

        // Soft delete
        content.IsDeleted = true;
        content.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");
        await _cacheService.RemoveByPatternAsync("Feed:*");
        await _cacheService.RemoveByPatternAsync("Trending:*");
        await _cacheService.RemoveByPatternAsync("Popular:*");

        // Hangfire: RediSearch index'ten sil
        BackgroundJob.Enqueue(() => DeleteContentIndexAsync(contentId));

        await _auditService.LogAsync(userId, "ContentDeleted", 
            JsonSerializer.Serialize(new { ContentId = contentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    // ========== LIKE/UNLIKE ==========

    public async Task<BaseResponse<bool>> LikeContentAsync(int contentId)
    {
        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted);

        if (content == null)
            return BaseResponse<bool>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        // Zaten beğenilmiş mi?
        var existingLike = await _context.Interactions
            .FirstOrDefaultAsync(i => i.UserId == userId && 
                                     i.ContentId == contentId && 
                                     i.Type == InteractionType.Like);

        if (existingLike != null)
            return BaseResponse<bool>.ErrorResponse("Already liked", ErrorCodes.ValidationFailed);

        // Like oluştur
        var like = new Interaction
        {
            UserId = userId,
            ContentId = contentId,
            Type = InteractionType.Like,
            CreatedAt = DateTime.UtcNow
        };

        _context.Interactions.Add(like);

        // Like count'u güncelle (optimistic update)
        content.LikesCount++;
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");
        await _cacheService.RemoveByPatternAsync("Trending:*");
        await _cacheService.RemoveByPatternAsync("Popular:*");
        await _cacheService.RemoveByPatternAsync("Feed:*");

        // SignalR notification (content author'a)
        if (content.AuthorId != userId)
        {
            await _notificationHub.Clients.Group($"User_{content.AuthorId}")
                .SendAsync("ContentLiked", new { ContentId = contentId, UserId = userId });
        }

        await _auditService.LogAsync(userId, "ContentLiked", 
            JsonSerializer.Serialize(new { ContentId = contentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> UnlikeContentAsync(int contentId)
    {
        var userId = _sessionService.GetUserId();

        var like = await _context.Interactions
            .FirstOrDefaultAsync(i => i.UserId == userId && 
                                     i.ContentId == contentId && 
                                     i.Type == InteractionType.Like);

        if (like == null)
            return BaseResponse<bool>.ErrorResponse("Not liked", ErrorCodes.NotFound);

        _context.Interactions.Remove(like);

        // Like count'u azalt
        var content = await _context.Contents.FindAsync(contentId);
        if (content != null && content.LikesCount > 0)
        {
            content.LikesCount--;
            await _context.SaveChangesAsync();
        }
        else
        {
            await _context.SaveChangesAsync();
        }

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");
        await _cacheService.RemoveByPatternAsync("Trending:*");
        await _cacheService.RemoveByPatternAsync("Popular:*");

        await _auditService.LogAsync(userId, "ContentUnliked", 
            JsonSerializer.Serialize(new { ContentId = contentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    // ========== COMMENT MANAGEMENT ==========

    public async Task<BaseResponse<CommentDto>> CreateCommentAsync(int contentId, CreateCommentRequest request)
    {
        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted);

        if (content == null)
            return BaseResponse<CommentDto>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        // Parent comment kontrolü (nested comment için)
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId && !c.IsDeleted);

            if (parentComment == null)
                return BaseResponse<CommentDto>.ErrorResponse("Parent comment not found", ErrorCodes.NotFound);
        }

        var comment = new Comment
        {
            ContentId = contentId,
            AuthorId = userId,
            Text = request.Text,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);

        // Comment count'u güncelle
        content.CommentCount++;
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _context.Comments.FindAsync(request.ParentCommentId);
            if (parentComment != null)
            {
                parentComment.RepliesCount++;
            }
        }

        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");
        await _cacheService.RemoveByPatternAsync("Feed:*");

        // SignalR notification (real-time comment)
        await _notificationHub.Clients.Group($"User_{content.AuthorId}")
            .SendAsync("NewComment", new { ContentId = contentId, CommentId = comment.Id, UserId = userId });

        await _auditService.LogAsync(userId, "CommentCreated", 
            JsonSerializer.Serialize(new { ContentId = contentId, CommentId = comment.Id }));

        var dto = await MapToCommentDtoAsync(comment, userId);
        return BaseResponse<CommentDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<PagedResponse<CommentDto>>> GetContentCommentsAsync(
        int contentId, 
        int page = 1, 
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"Content:{contentId}:Comments:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<CommentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<CommentDto>>.SuccessResponse(cached);
        }

        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.ContentId == contentId && !c.IsDeleted && c.ParentCommentId == null)
            .Include(c => c.Author)
            .Include(c => c.Replies.Where(r => !r.IsDeleted).OrderBy(r => r.CreatedAt).Take(5))
                .ThenInclude(r => r.Author)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<CommentDto>();
        foreach (var comment in comments)
        {
            var dto = await MapToCommentDtoAsync(comment, userId);
            dtos.Add(dto);
        }

        var response = new PagedResponse<CommentDto>(dtos, totalCount, page, limit);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        return BaseResponse<PagedResponse<CommentDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<bool>> UpdateCommentAsync(int commentId, UpdateCommentRequest request)
    {
        var userId = _sessionService.GetUserId();

        var comment = await _context.Comments
            .Include(c => c.Content)
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (comment == null)
            return BaseResponse<bool>.ErrorResponse("Comment not found", ErrorCodes.NotFound);

        // Yetki kontrolü
        if (comment.AuthorId != userId)
            return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);

        comment.Text = request.Text;
        comment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Content:{comment.ContentId}:*");

        await _auditService.LogAsync(userId, "CommentUpdated", 
            JsonSerializer.Serialize(new { CommentId = commentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> DeleteCommentAsync(int commentId)
    {
        var userId = _sessionService.GetUserId();

        var comment = await _context.Comments
            .Include(c => c.Content)
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (comment == null)
            return BaseResponse<bool>.ErrorResponse("Comment not found", ErrorCodes.NotFound);

        // Yetki kontrolü
        if (comment.AuthorId != userId)
            return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);

        // Soft delete
        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;

        // Comment count'u azalt
        if (comment.Content != null)
        {
            comment.Content.CommentCount = Math.Max(0, comment.Content.CommentCount - 1);
        }

        // Parent comment'in replies count'unu azalt
        if (comment.ParentCommentId.HasValue)
        {
            var parentComment = await _context.Comments.FindAsync(comment.ParentCommentId);
            if (parentComment != null)
            {
                parentComment.RepliesCount = Math.Max(0, parentComment.RepliesCount - 1);
            }
        }

        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Content:{comment.ContentId}:*");

        await _auditService.LogAsync(userId, "CommentDeleted", 
            JsonSerializer.Serialize(new { CommentId = commentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    // ========== FOLLOW/UNFOLLOW ==========

    public async Task<BaseResponse<bool>> FollowUserAsync(int targetUserId)
    {
        var userId = _sessionService.GetUserId();

        if (userId == targetUserId)
            return BaseResponse<bool>.ErrorResponse("Cannot follow yourself", ErrorCodes.ValidationFailed);

        // Zaten takip ediliyor mu?
        var existingFollow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == targetUserId);

        if (existingFollow != null)
            return BaseResponse<bool>.ErrorResponse("Already following", ErrorCodes.ValidationFailed);

        var follow = new Follow
        {
            FollowerId = userId,
            FollowingId = targetUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Follows.Add(follow);

        // Denormalized counts güncelle
        var follower = await _context.Users.FindAsync(userId);
        var following = await _context.Users.FindAsync(targetUserId);
        if (follower != null) follower.FollowingCount++;
        if (following != null) following.FollowerCount++;

        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Following:*");
        await _cacheService.RemoveByPatternAsync($"User:{targetUserId}:Followers:*");
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Feed:*");
        await _cacheService.RemoveByPatternAsync("Recommended:*");

        // Notification (takip edilen kullanıcıya)
        await _notificationService.SendNotificationAsync(
            targetUserId,
            "Yeni Takipçi",
            $"{follower?.FullName ?? "Birisi"} sizi takip etmeye başladı",
            NotificationType.Follow,
            $"/user/{userId}");

        // SignalR notification (real-time)
        await _notificationHub.Clients.Group($"User_{targetUserId}")
            .SendAsync("NewFollower", new { FollowerId = userId, FollowerName = follower?.FullName ?? "Birisi" });

        await _auditService.LogAsync(userId, "UserFollowed", 
            JsonSerializer.Serialize(new { FollowingId = targetUserId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> UnfollowUserAsync(int targetUserId)
    {
        var userId = _sessionService.GetUserId();

        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == targetUserId);

        if (follow == null)
            return BaseResponse<bool>.ErrorResponse("Not following", ErrorCodes.NotFound);

        _context.Follows.Remove(follow);

        // Denormalized counts güncelle
        var follower = await _context.Users.FindAsync(userId);
        var following = await _context.Users.FindAsync(targetUserId);
        if (follower != null && follower.FollowingCount > 0) follower.FollowingCount--;
        if (following != null && following.FollowerCount > 0) following.FollowerCount--;

        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Following:*");
        await _cacheService.RemoveByPatternAsync($"User:{targetUserId}:Followers:*");
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Feed:*");

        await _auditService.LogAsync(userId, "UserUnfollowed", 
            JsonSerializer.Serialize(new { FollowingId = targetUserId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    // ========== SAVE/UNSAVE ==========

    public async Task<BaseResponse<bool>> SaveContentAsync(int contentId)
    {
        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted);

        if (content == null)
            return BaseResponse<bool>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        // Zaten kaydedilmiş mi?
        var existingSave = await _context.Interactions
            .FirstOrDefaultAsync(i => i.UserId == userId && 
                                     i.ContentId == contentId && 
                                     i.Type == InteractionType.Save);

        if (existingSave != null)
            return BaseResponse<bool>.ErrorResponse("Already saved", ErrorCodes.ValidationFailed);

        var save = new Interaction
        {
            UserId = userId,
            ContentId = contentId,
            Type = InteractionType.Save,
            CreatedAt = DateTime.UtcNow
        };

        _context.Interactions.Add(save);

        // Saves count'u güncelle
        content.SavesCount++;
        await _context.SaveChangesAsync();

        // Cache invalidation (saved contents list)
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Saved:*");

        await _auditService.LogAsync(userId, "ContentSaved", 
            JsonSerializer.Serialize(new { ContentId = contentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> UnsaveContentAsync(int contentId)
    {
        var userId = _sessionService.GetUserId();

        var save = await _context.Interactions
            .FirstOrDefaultAsync(i => i.UserId == userId && 
                                     i.ContentId == contentId && 
                                     i.Type == InteractionType.Save);

        if (save == null)
            return BaseResponse<bool>.ErrorResponse("Not saved", ErrorCodes.NotFound);

        _context.Interactions.Remove(save);

        // Saves count'u azalt
        var content = await _context.Contents.FindAsync(contentId);
        if (content != null && content.SavesCount > 0)
        {
            content.SavesCount--;
            await _context.SaveChangesAsync();
        }
        else
        {
            await _context.SaveChangesAsync();
        }

        await _cacheService.RemoveByPatternAsync($"User:{userId}:Saved:*");

        await _auditService.LogAsync(userId, "ContentUnsaved", 
            JsonSerializer.Serialize(new { ContentId = contentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    // ========== SHARE ==========

    public async Task<BaseResponse<bool>> ShareContentAsync(int contentId)
    {
        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted);

        if (content == null)
            return BaseResponse<bool>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        // Share interaction oluştur
        var share = new Interaction
        {
            UserId = userId,
            ContentId = contentId,
            Type = InteractionType.Share,
            CreatedAt = DateTime.UtcNow
        };

        _context.Interactions.Add(share);

        // Share count'u güncelle
        content.ShareCount++;
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");
        await _cacheService.RemoveByPatternAsync("Trending:*");

        await _auditService.LogAsync(userId, "ContentShared", 
            JsonSerializer.Serialize(new { ContentId = contentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    // ========== BLOCK/MUTE ==========

    public async Task<BaseResponse<bool>> BlockUserAsync(int targetUserId)
    {
        var userId = _sessionService.GetUserId();

        if (userId == targetUserId)
            return BaseResponse<bool>.ErrorResponse("Cannot block yourself", ErrorCodes.ValidationFailed);

        // Zaten engellenmiş mi?
        var existingBlock = await _context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerId == userId && b.BlockedId == targetUserId);

        if (existingBlock != null)
            return BaseResponse<bool>.ErrorResponse("Already blocked", ErrorCodes.ValidationFailed);

        var block = new Block
        {
            BlockerId = userId,
            BlockedId = targetUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Blocks.Add(block);

        // Takip varsa kaldır (her iki yönde)
        var follows = await _context.Follows
            .Where(f => (f.FollowerId == userId && f.FollowingId == targetUserId) ||
                       (f.FollowerId == targetUserId && f.FollowingId == userId))
            .ToListAsync();

        _context.Follows.RemoveRange(follows);

        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:*");
        await _cacheService.RemoveByPatternAsync($"User:{targetUserId}:*");
        await _cacheService.RemoveByPatternAsync("Feed:*");

        await _auditService.LogAsync(userId, "UserBlocked", 
            JsonSerializer.Serialize(new { BlockedId = targetUserId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> UnblockUserAsync(int targetUserId)
    {
        var userId = _sessionService.GetUserId();

        var block = await _context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerId == userId && b.BlockedId == targetUserId);

        if (block == null)
            return BaseResponse<bool>.ErrorResponse("Not blocked", ErrorCodes.NotFound);

        _context.Blocks.Remove(block);
        await _context.SaveChangesAsync();

        await _cacheService.RemoveByPatternAsync($"User:{userId}:*");
        await _cacheService.RemoveByPatternAsync($"User:{targetUserId}:*");

        await _auditService.LogAsync(userId, "UserUnblocked", 
            JsonSerializer.Serialize(new { UnblockedId = targetUserId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> MuteUserAsync(int targetUserId)
    {
        var userId = _sessionService.GetUserId();

        if (userId == targetUserId)
            return BaseResponse<bool>.ErrorResponse("Cannot mute yourself", ErrorCodes.ValidationFailed);

        // Zaten sessizleştirilmiş mi?
        var existingMute = await _context.Mutes
            .FirstOrDefaultAsync(m => m.UserId == userId && m.MutedUserId == targetUserId);

        if (existingMute != null)
            return BaseResponse<bool>.ErrorResponse("Already muted", ErrorCodes.ValidationFailed);

        var mute = new Mute
        {
            UserId = userId,
            MutedUserId = targetUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Mutes.Add(mute);
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Feed:*");
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Muted:*");

        await _auditService.LogAsync(userId, "UserMuted", 
            JsonSerializer.Serialize(new { MutedUserId = targetUserId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> UnmuteUserAsync(int targetUserId)
    {
        var userId = _sessionService.GetUserId();

        var mute = await _context.Mutes
            .FirstOrDefaultAsync(m => m.UserId == userId && m.MutedUserId == targetUserId);

        if (mute == null)
            return BaseResponse<bool>.ErrorResponse("Not muted", ErrorCodes.NotFound);

        _context.Mutes.Remove(mute);
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Feed:*");
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Muted:*");

        await _auditService.LogAsync(userId, "UserUnmuted", 
            JsonSerializer.Serialize(new { UnmutedUserId = targetUserId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    // ========== STORIES ==========

    public async Task<BaseResponse<StoryDto>> CreateStoryAsync(CreateStoryRequest request)
    {
        var userId = _sessionService.GetUserId();

        // Validation
        if (string.IsNullOrEmpty(request.ImageUrl) && 
            string.IsNullOrEmpty(request.VideoUrl) && 
            string.IsNullOrEmpty(request.Text))
        {
            return BaseResponse<StoryDto>.ErrorResponse(
                "At least one of imageUrl, videoUrl, or text is required", 
                ErrorCodes.ValidationFailed);
        }

        if (!string.IsNullOrEmpty(request.Text) && request.Text.Length > 200)
        {
            return BaseResponse<StoryDto>.ErrorResponse(
                "Text cannot exceed 200 characters", 
                ErrorCodes.ValidationFailed);
        }

        var story = new Story
        {
            AuthorId = userId,
            ImageUrl = request.ImageUrl,
            VideoUrl = request.VideoUrl,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24) // 24 saat sonra expire
        };

        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Stories:*");
        await _cacheService.RemoveByPatternAsync("Stories:Following:*");
        await _cacheService.RemoveByPatternAsync("Stories:Active:*");

        // SignalR: Takipçilere bildirim
        var followers = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowingId == userId)
            .Select(f => f.FollowerId)
            .ToListAsync();

        foreach (var followerId in followers)
        {
            await _notificationHub.Clients.Group($"User_{followerId}")
                .SendAsync("NewStory", new { AuthorId = userId, StoryId = story.Id });
        }

        await _auditService.LogAsync(userId, "StoryCreated", 
            JsonSerializer.Serialize(new { StoryId = story.Id }));

        var dto = await MapToStoryDtoAsync(story, userId);
        return BaseResponse<StoryDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<List<StoryGroupDto>>> GetStoriesAsync(bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{userId}:Stories:Following";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<StoryGroupDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<List<StoryGroupDto>>.SuccessResponse(cached);
        }

        // Takip edilenler + kendi story'lerim
        var followingIds = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        followingIds.Add(userId); // Kendi story'lerimizi de ekle

        // Aktif story'ler (expire olmamış)
        var now = DateTime.UtcNow;
        var activeStories = await _context.Stories
            .AsNoTracking()
            .Where(s => followingIds.Contains(s.AuthorId) && 
                       !s.IsDeleted &&
                       s.ExpiresAt > now)
            .Include(s => s.Author)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        // Author'a göre grupla
        var storyGroups = activeStories
            .GroupBy(s => s.AuthorId)
            .Select(g => new StoryGroupDto
            {
                AuthorId = g.Key,
                AuthorName = g.First().Author.FullName,
                AuthorImageUrl = g.First().Author.ProfileImageUrl,
                Stories = g.Select(s => MapToStoryDto(s, userId)).ToList()
            })
            .OrderByDescending(g => g.Stories.Max(s => s.CreatedAt))
            .ToList();

        await _cacheService.SetAsync(cacheKey, storyGroups, TimeSpan.FromMinutes(1)); // Çok dinamik, 1 dakika cache

        return BaseResponse<List<StoryGroupDto>>.SuccessResponse(storyGroups);
    }

    public async Task<BaseResponse<StoryDto>> GetStoryByIdAsync(int storyId, bool markAsViewed = true)
    {
        var userId = _sessionService.GetUserId();

        var story = await _context.Stories
            .AsNoTracking()
            .Include(s => s.Author)
            .FirstOrDefaultAsync(s => s.Id == storyId && !s.IsDeleted);

        if (story == null)
            return BaseResponse<StoryDto>.ErrorResponse("Story not found", ErrorCodes.NotFound);

        // Expire kontrolü
        if (story.ExpiresAt <= DateTime.UtcNow)
            return BaseResponse<StoryDto>.ErrorResponse("Story has expired", ErrorCodes.NotFound);

        // Privacy kontrolü
        if (story.AuthorId != userId)
        {
            var isFollowing = await _context.Follows
                .AsNoTracking()
                .AnyAsync(f => f.FollowerId == userId && f.FollowingId == story.AuthorId);

            if (!isFollowing)
            {
                return BaseResponse<StoryDto>.ErrorResponse(
                    "You must follow this user to view their stories", 
                    ErrorCodes.AccessDenied);
            }
        }

        // View tracking
        if (markAsViewed)
        {
            var existingView = await _context.StoryViews
                .FirstOrDefaultAsync(v => v.StoryId == storyId && v.UserId == userId);

            if (existingView == null)
            {
                var view = new StoryView
                {
                    StoryId = storyId,
                    UserId = userId,
                    ViewedAt = DateTime.UtcNow
                };
                _context.StoryViews.Add(view);

                // Views count'u güncelle
                story.ViewsCount++;
                await _context.SaveChangesAsync();

                // Cache invalidation
                await _cacheService.RemoveByPatternAsync($"Story:{storyId}:*");
            }
        }

        var dto = MapToStoryDto(story, userId);
        return BaseResponse<StoryDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<bool>> DeleteStoryAsync(int storyId)
    {
        var userId = _sessionService.GetUserId();

        var story = await _context.Stories
            .FirstOrDefaultAsync(s => s.Id == storyId);

        if (story == null)
            return BaseResponse<bool>.ErrorResponse("Story not found", ErrorCodes.NotFound);

        // Yetki kontrolü
        if (story.AuthorId != userId)
            return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.AccessDenied);

        // Soft delete
        story.IsDeleted = true;
        story.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Stories:*");
        await _cacheService.RemoveByPatternAsync($"Story:{storyId}:*");
        await _cacheService.RemoveByPatternAsync("Stories:*");

        await _auditService.LogAsync(userId, "StoryDeleted", 
            JsonSerializer.Serialize(new { StoryId = storyId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> ReactToStoryAsync(int storyId, ReactToStoryRequest request)
    {
        var userId = _sessionService.GetUserId();

        var story = await _context.Stories
            .FirstOrDefaultAsync(s => s.Id == storyId && !s.IsDeleted);

        if (story == null)
            return BaseResponse<bool>.ErrorResponse("Story not found", ErrorCodes.NotFound);

        // Expire kontrolü
        if (story.ExpiresAt <= DateTime.UtcNow)
            return BaseResponse<bool>.ErrorResponse("Story has expired", ErrorCodes.NotFound);

        // Zaten tepki vermiş mi?
        var existingReaction = await _context.StoryReactions
            .FirstOrDefaultAsync(r => r.StoryId == storyId && r.UserId == userId);

        if (existingReaction != null)
        {
            // Tepkiyi güncelle
            existingReaction.Reaction = request.Reaction;
            existingReaction.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            // Yeni tepki
            var reaction = new StoryReaction
            {
                StoryId = storyId,
                UserId = userId,
                Reaction = request.Reaction,
                CreatedAt = DateTime.UtcNow
            };
            _context.StoryReactions.Add(reaction);

            // Reactions count'u güncelle
            story.ReactionsCount++;
        }

        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Story:{storyId}:*");

        // SignalR: Story sahibine bildirim
        await _notificationHub.Clients.Group($"User_{story.AuthorId}")
            .SendAsync("StoryReaction", new { StoryId = storyId, UserId = userId, Reaction = request.Reaction });

        await _auditService.LogAsync(userId, "StoryReacted", 
            JsonSerializer.Serialize(new { StoryId = storyId, Reaction = request.Reaction }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    // ========== HELPER METHODS ==========

    private async Task<ContentDto> MapToContentDtoAsync(Content content, int currentUserId)
    {
        // IsLiked ve IsSaved kontrolü
        var isLiked = await _context.Interactions
            .AnyAsync(i => i.UserId == currentUserId && i.ContentId == content.Id && i.Type == InteractionType.Like);

        var isSaved = await _context.Interactions
            .AnyAsync(i => i.UserId == currentUserId && i.ContentId == content.Id && i.Type == InteractionType.Save);

        var tags = !string.IsNullOrEmpty(content.TagsJson)
            ? JsonSerializer.Deserialize<List<string>>(content.TagsJson) ?? new List<string>()
            : new List<string>();

        return new ContentDto
        {
            Id = content.Id,
            AuthorId = content.AuthorId,
            AuthorName = content.Author.FullName,
            AuthorImageUrl = content.Author.ProfileImageUrl,
            ContentType = content.ContentType,
            Title = content.Title,
            Description = content.Description,
            ImageUrl = content.ImageUrl,
            VideoUrl = content.VideoUrl,
            FileUrl = content.FileUrl,
            LessonId = content.LessonId,
            LessonName = content.Lesson?.Name,
            TopicId = content.TopicId,
            TopicName = content.Topic?.Name,
            Difficulty = content.Difficulty,
            Tags = tags,
            ViewsCount = content.ViewsCount,
            LikesCount = content.LikesCount,
            CommentCount = content.CommentCount,
            ShareCount = content.ShareCount,
            SavesCount = content.SavesCount,
            IsSolved = content.IsSolved,
            IsLiked = isLiked,
            IsSaved = isSaved,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt
        };
    }

    private async Task<ContentDetailDto> MapToContentDetailDtoAsync(Content content, int currentUserId)
    {
        var baseDto = await MapToContentDtoAsync(content, currentUserId);

        // Comments mapping
        var commentDtos = new List<CommentDto>();
        foreach (var comment in content.Comments.Where(c => !c.IsDeleted))
        {
            var commentDto = await MapToCommentDtoAsync(comment, currentUserId);
            commentDtos.Add(commentDto);
        }

        return new ContentDetailDto
        {
            Id = baseDto.Id,
            AuthorId = baseDto.AuthorId,
            AuthorName = baseDto.AuthorName,
            AuthorImageUrl = baseDto.AuthorImageUrl,
            ContentType = baseDto.ContentType,
            Title = baseDto.Title,
            Description = baseDto.Description,
            ImageUrl = baseDto.ImageUrl,
            VideoUrl = baseDto.VideoUrl,
            FileUrl = baseDto.FileUrl,
            LessonId = baseDto.LessonId,
            LessonName = baseDto.LessonName,
            TopicId = baseDto.TopicId,
            TopicName = baseDto.TopicName,
            Difficulty = baseDto.Difficulty,
            Tags = baseDto.Tags,
            ViewsCount = baseDto.ViewsCount,
            LikesCount = baseDto.LikesCount,
            CommentCount = baseDto.CommentCount,
            ShareCount = baseDto.ShareCount,
            SavesCount = baseDto.SavesCount,
            IsSolved = baseDto.IsSolved,
            IsLiked = baseDto.IsLiked,
            IsSaved = baseDto.IsSaved,
            CreatedAt = baseDto.CreatedAt,
            UpdatedAt = baseDto.UpdatedAt,
            Comments = commentDtos,
            IsAuthor = content.AuthorId == currentUserId
        };
    }

    private async Task<CommentDto> MapToCommentDtoAsync(Comment comment, int currentUserId)
    {
        var isLiked = await _context.Interactions
            .AnyAsync(i => i.UserId == currentUserId && 
                          i.ContentId == comment.ContentId && 
                          i.Type == InteractionType.Like);

        var replies = new List<CommentDto>();
        if (comment.Replies != null && comment.Replies.Any())
        {
            foreach (var reply in comment.Replies.Where(r => !r.IsDeleted))
            {
                var replyDto = await MapToCommentDtoAsync(reply, currentUserId);
                replies.Add(replyDto);
            }
        }

        return new CommentDto
        {
            Id = comment.Id,
            ContentId = comment.ContentId,
            AuthorId = comment.AuthorId,
            AuthorName = comment.Author.FullName,
            AuthorImageUrl = comment.Author.ProfileImageUrl,
            Text = comment.Text,
            ParentCommentId = comment.ParentCommentId,
            LikesCount = comment.LikesCount,
            RepliesCount = comment.RepliesCount,
            IsLiked = isLiked,
            Replies = replies.Any() ? replies : null,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    private StoryDto MapToStoryDto(Story story, int currentUserId)
    {
        var isViewed = _context.StoryViews
            .Any(v => v.StoryId == story.Id && v.UserId == currentUserId);

        return new StoryDto
        {
            Id = story.Id,
            AuthorId = story.AuthorId,
            AuthorName = story.Author.FullName,
            AuthorImageUrl = story.Author.ProfileImageUrl,
            ImageUrl = story.ImageUrl,
            VideoUrl = story.VideoUrl,
            Text = story.Text,
            ViewsCount = story.ViewsCount,
            ReactionsCount = story.ReactionsCount,
            IsViewed = isViewed,
            CreatedAt = story.CreatedAt,
            ExpiresAt = story.ExpiresAt
        };
    }

    private async Task<StoryDto> MapToStoryDtoAsync(Story story, int currentUserId)
    {
        var isViewed = await _context.StoryViews
            .AnyAsync(v => v.StoryId == story.Id && v.UserId == currentUserId);

        return new StoryDto
        {
            Id = story.Id,
            AuthorId = story.AuthorId,
            AuthorName = story.Author.FullName,
            AuthorImageUrl = story.Author.ProfileImageUrl,
            ImageUrl = story.ImageUrl,
            VideoUrl = story.VideoUrl,
            Text = story.Text,
            ViewsCount = story.ViewsCount,
            ReactionsCount = story.ReactionsCount,
            IsViewed = isViewed,
            CreatedAt = story.CreatedAt,
            ExpiresAt = story.ExpiresAt
        };
    }

    // ========== FEED SYSTEM ==========

    public async Task<BaseResponse<PagedResponse<ContentDto>>> GetFeedAsync(
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{userId}:Feed:Personalized:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
        }

        // Engellenen ve sessizleştirilen kullanıcıları filtrele
        var blockedIds = await _context.Blocks
            .AsNoTracking()
            .Where(b => b.BlockerId == userId)
            .Select(b => b.BlockedId)
            .ToListAsync();

        var mutedIds = await _context.Mutes
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.MutedUserId)
            .ToListAsync();

        var excludedIds = blockedIds.Union(mutedIds).ToList();

        // Takip edilenlerin içerikleri + popüler içerikler (karma feed)
        var followingIds = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var query = _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted && 
                       !excludedIds.Contains(c.AuthorId) &&
                       (followingIds.Contains(c.AuthorId) || 
                        c.LikesCount >= 10 || 
                        c.CommentCount >= 5))
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .OrderByDescending(c => c.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var contents = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var content in contents)
        {
            var dto = await MapToContentDtoAsync(content, userId);
            dtos.Add(dto);
        }

        var response = new PagedResponse<ContentDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<PagedResponse<ContentDto>>> GetFollowingFeedAsync(
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{userId}:Feed:Following:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
        }

        var followingIds = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        if (!followingIds.Any())
            return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(
                new PagedResponse<ContentDto>(new List<ContentDto>(), 0, page, limit));

        var query = _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted && followingIds.Contains(c.AuthorId))
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .OrderByDescending(c => c.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var contents = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var content in contents)
        {
            var dto = await MapToContentDtoAsync(content, userId);
            dtos.Add(dto);
        }

        var response = new PagedResponse<ContentDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<PagedResponse<ContentDto>>> GetForYouFeedAsync(
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        // "Senin için" feed = GetRecommendedContentsAsync ile aynı mantık
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{userId}:Feed:ForYou:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
        }

        // Recommendation algoritması (GetRecommendedContentsAsync ile aynı mantık)
        var likedLessonIds = await _context.Interactions
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.Type == InteractionType.Like)
            .Include(i => i.Content)
            .Where(i => i.Content != null && i.Content.LessonId.HasValue)
            .Select(i => i.Content!.LessonId!.Value)
            .Distinct()
            .ToListAsync();

        var followingIds = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var query = _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.AuthorId != userId)
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .AsQueryable();

        // Öncelik: Takip edilenler > Beğenilen dersler > Popüler
        if (followingIds.Any())
        {
            query = query.OrderByDescending(c => followingIds.Contains(c.AuthorId))
                .ThenByDescending(c => likedLessonIds.Contains(c.LessonId ?? 0))
                .ThenByDescending(c => c.LikesCount + c.CommentCount * 2);
        }
        else if (likedLessonIds.Any())
        {
            query = query.OrderByDescending(c => likedLessonIds.Contains(c.LessonId ?? 0))
                .ThenByDescending(c => c.LikesCount + c.CommentCount * 2);
        }
        else
        {
            query = query.OrderByDescending(c => c.LikesCount + c.CommentCount * 2);
        }

        var totalCount = await query.CountAsync();
        var contents = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var content in contents)
        {
            var dto = await MapToContentDtoAsync(content, userId);
            dtos.Add(dto);
        }

        var response = new PagedResponse<ContentDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15));

        return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<PagedResponse<ContentDto>>> GetTrendingContentsAsync(
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"Trending:Contents:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
        }

        // Son 24 saatte en çok beğenilen/yorumlanan içerikler
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);

        var query = _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.CreatedAt >= oneDayAgo)
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .OrderByDescending(c => c.LikesCount * 2 + c.CommentCount * 3 + c.ViewsCount)
            .ThenByDescending(c => c.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var contents = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var content in contents)
        {
            var dto = await MapToContentDtoAsync(content, userId);
            dtos.Add(dto);
        }

        var response = new PagedResponse<ContentDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<PagedResponse<ContentDto>>> GetPopularContentsAsync(
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"Popular:Contents:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
        }

        var query = _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .OrderByDescending(c => c.LikesCount + c.CommentCount * 2)
            .ThenByDescending(c => c.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var contents = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var content in contents)
        {
            var dto = await MapToContentDtoAsync(content, userId);
            dtos.Add(dto);
        }

        var response = new PagedResponse<ContentDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15));

        return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<List<ContentDto>>> GetRecommendedContentsAsync(
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{userId}:Recommendations:Limit{limit}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<List<ContentDto>>.SuccessResponse(cached);
        }

        // Basit recommendation algoritması:
        // 1. Kullanıcının beğendiği içeriklerin ders/konu analizi
        // 2. Takip ettiği kullanıcıların paylaştığı içerikler
        // 3. Trend içerikler

        var likedLessonIds = await _context.Interactions
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.Type == InteractionType.Like)
            .Include(i => i.Content)
            .Where(i => i.Content != null && i.Content.LessonId.HasValue)
            .Select(i => i.Content!.LessonId!.Value)
            .Distinct()
            .ToListAsync();

        var followingIds = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var query = _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.AuthorId != userId)
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .AsQueryable();

        // Öncelik: Takip edilenler > Beğenilen dersler > Popüler
        if (followingIds.Any())
        {
            query = query.OrderByDescending(c => followingIds.Contains(c.AuthorId))
                .ThenByDescending(c => likedLessonIds.Contains(c.LessonId ?? 0))
                .ThenByDescending(c => c.LikesCount + c.CommentCount * 2);
        }
        else if (likedLessonIds.Any())
        {
            query = query.OrderByDescending(c => likedLessonIds.Contains(c.LessonId ?? 0))
                .ThenByDescending(c => c.LikesCount + c.CommentCount * 2);
        }
        else
        {
            query = query.OrderByDescending(c => c.LikesCount + c.CommentCount * 2);
        }

        var contents = await query
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var content in contents)
        {
            var dto = await MapToContentDtoAsync(content, userId);
            dtos.Add(dto);
        }

        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(15));

        return BaseResponse<List<ContentDto>>.SuccessResponse(dtos);
    }

    // ========== USER CONTENTS ==========

    public async Task<BaseResponse<PagedResponse<ContentDto>>> GetUserContentsAsync(
        int targetUserId,
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{targetUserId}:Contents:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
        }

        var query = _context.Contents
            .AsNoTracking()
            .Where(c => c.AuthorId == targetUserId && !c.IsDeleted)
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .OrderByDescending(c => c.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var contents = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var content in contents)
        {
            var dto = await MapToContentDtoAsync(content, userId);
            dtos.Add(dto);
        }

        var response = new PagedResponse<ContentDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<PagedResponse<ContentDto>>> GetSavedContentsAsync(
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{userId}:Saved:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
        }

        var query = _context.Interactions
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.Type == InteractionType.Save)
            .Include(i => i.Content)
                .ThenInclude(c => c!.Author)
            .Include(i => i.Content)
                .ThenInclude(c => c!.Lesson)
            .Include(i => i.Content)
                .ThenInclude(c => c!.Topic)
            .Where(i => i.Content != null && !i.Content.IsDeleted)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var saves = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var save in saves)
        {
            if (save.Content != null)
            {
                var dto = await MapToContentDtoAsync(save.Content, userId);
                dtos.Add(dto);
            }
        }

        var response = new PagedResponse<ContentDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
    }

    // ========== FOLLOW SYSTEM (EXTENDED) ==========

    public async Task<BaseResponse<PagedResponse<UserProfileSocialDto>>> GetFollowersAsync(
        int targetUserId,
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{targetUserId}:Followers:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<UserProfileSocialDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<UserProfileSocialDto>>.SuccessResponse(cached);
        }

        var query = _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowingId == targetUserId)
            .Include(f => f.Follower)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var follows = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<UserProfileSocialDto>();
        foreach (var follow in follows)
        {
            var isFollowing = await _context.Follows
                .AsNoTracking()
                .AnyAsync(f => f.FollowerId == userId && f.FollowingId == follow.FollowerId);

            var isBlocked = await _context.Blocks
                .AsNoTracking()
                .AnyAsync(b => b.BlockerId == userId && b.BlockedId == follow.FollowerId);

            var contentCount = await _context.Contents
                .AsNoTracking()
                .CountAsync(c => c.AuthorId == follow.FollowerId && !c.IsDeleted);

            dtos.Add(new UserProfileSocialDto
            {
                UserId = follow.FollowerId,
                Username = follow.Follower.Username,
                FullName = follow.Follower.FullName,
                ProfileImageUrl = follow.Follower.ProfileImageUrl,
                FollowerCount = follow.Follower.FollowerCount,
                FollowingCount = follow.Follower.FollowingCount,
                ContentCount = contentCount,
                IsFollowing = isFollowing,
                IsBlocked = isBlocked,
                IsMuted = false
            });
        }

        var response = new PagedResponse<UserProfileSocialDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        return BaseResponse<PagedResponse<UserProfileSocialDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<PagedResponse<UserProfileSocialDto>>> GetFollowingAsync(
        int targetUserId,
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{targetUserId}:Following:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<UserProfileSocialDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<UserProfileSocialDto>>.SuccessResponse(cached);
        }

        var query = _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == targetUserId)
            .Include(f => f.Following)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var follows = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<UserProfileSocialDto>();
        foreach (var follow in follows)
        {
            var isFollowing = await _context.Follows
                .AsNoTracking()
                .AnyAsync(f => f.FollowerId == userId && f.FollowingId == follow.FollowingId);

            var isBlocked = await _context.Blocks
                .AsNoTracking()
                .AnyAsync(b => b.BlockerId == userId && b.BlockedId == follow.FollowingId);

            var contentCount = await _context.Contents
                .AsNoTracking()
                .CountAsync(c => c.AuthorId == follow.FollowingId && !c.IsDeleted);

            dtos.Add(new UserProfileSocialDto
            {
                UserId = follow.FollowingId,
                Username = follow.Following.Username,
                FullName = follow.Following.FullName,
                ProfileImageUrl = follow.Following.ProfileImageUrl,
                FollowerCount = follow.Following.FollowerCount,
                FollowingCount = follow.Following.FollowingCount,
                ContentCount = contentCount,
                IsFollowing = isFollowing,
                IsBlocked = isBlocked,
                IsMuted = false
            });
        }

        var response = new PagedResponse<UserProfileSocialDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        return BaseResponse<PagedResponse<UserProfileSocialDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<UserProfileSocialDto>> GetUserProfileSocialAsync(int targetUserId)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{targetUserId}:ProfileSocial";
        var cached = await _cacheService.GetAsync<UserProfileSocialDto>(cacheKey);
        if (cached != null)
            return BaseResponse<UserProfileSocialDto>.SuccessResponse(cached);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == targetUserId);

        if (user == null)
            return BaseResponse<UserProfileSocialDto>.ErrorResponse("User not found", ErrorCodes.NotFound);

        var isFollowing = await _context.Follows
            .AsNoTracking()
            .AnyAsync(f => f.FollowerId == userId && f.FollowingId == targetUserId);

        var isBlocked = await _context.Blocks
            .AsNoTracking()
            .AnyAsync(b => b.BlockerId == userId && b.BlockedId == targetUserId);

        var isMuted = await _context.Mutes
            .AsNoTracking()
            .AnyAsync(m => m.UserId == userId && m.MutedUserId == targetUserId);

        var contentCount = await _context.Contents
            .AsNoTracking()
            .CountAsync(c => c.AuthorId == targetUserId && !c.IsDeleted);

        var dto = new UserProfileSocialDto
        {
            UserId = targetUserId,
            Username = user.Username,
            FullName = user.FullName,
            ProfileImageUrl = user.ProfileImageUrl,
            FollowerCount = user.FollowerCount,
            FollowingCount = user.FollowingCount,
            ContentCount = contentCount,
            IsFollowing = isFollowing,
            IsBlocked = isBlocked,
            IsMuted = isMuted
        };

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));

        return BaseResponse<UserProfileSocialDto>.SuccessResponse(dto);
    }

    // ========== COMMENT REPLIES ==========

    public async Task<BaseResponse<PagedResponse<CommentDto>>> GetCommentRepliesAsync(
        int commentId,
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"Comment:{commentId}:Replies:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<CommentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<CommentDto>>.SuccessResponse(cached);
        }

        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.ParentCommentId == commentId && !c.IsDeleted)
            .Include(c => c.Author)
            .OrderBy(c => c.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var comments = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<CommentDto>();
        foreach (var comment in comments)
        {
            var dto = await MapToCommentDtoAsync(comment, userId);
            dtos.Add(dto);
        }

        var response = new PagedResponse<CommentDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        return BaseResponse<PagedResponse<CommentDto>>.SuccessResponse(response);
    }

    // ========== HASHTAGS & TAGS ==========

    public async Task<BaseResponse<List<HashtagDto>>> GetTrendingHashtagsAsync(int limit = 20)
    {
        var cacheKey = "Trending:Hashtags";
        var cached = await _cacheService.GetAsync<List<HashtagDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<HashtagDto>>.SuccessResponse(cached);

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var allContents = await _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.CreatedAt >= sevenDaysAgo && !string.IsNullOrEmpty(c.TagsJson))
            .Select(c => c.TagsJson)
            .ToListAsync();

        var hashtagCounts = new Dictionary<string, int>();
        foreach (var tagsJson in allContents)
        {
            if (string.IsNullOrEmpty(tagsJson)) continue;
            var tags = JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>();
            foreach (var tag in tags)
            {
                var normalizedTag = tag.ToLower().TrimStart('#');
                if (!string.IsNullOrEmpty(normalizedTag))
                {
                    hashtagCounts.TryGetValue(normalizedTag, out var count);
                    hashtagCounts[normalizedTag] = count + 1;
                }
            }
        }

        var trending = hashtagCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(limit)
            .Select(kvp => new HashtagDto
            {
                Tag = kvp.Key,
                UsageCount = kvp.Value
            })
            .ToList();

        await _cacheService.SetAsync(cacheKey, trending, TimeSpan.FromMinutes(30));

        return BaseResponse<List<HashtagDto>>.SuccessResponse(trending);
    }

    public async Task<BaseResponse<HashtagDetailDto>> GetHashtagDetailAsync(string tag)
    {
        var normalizedTag = tag.ToLower().TrimStart('#');

        var cacheKey = $"Hashtag:{normalizedTag}:Detail";
        var cached = await _cacheService.GetAsync<HashtagDetailDto>(cacheKey);
        if (cached != null)
            return BaseResponse<HashtagDetailDto>.SuccessResponse(cached);

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var recentUsageCount = await _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted && 
                       c.CreatedAt >= sevenDaysAgo && 
                       !string.IsNullOrEmpty(c.TagsJson) &&
                       c.TagsJson.ToLower().Contains(normalizedTag))
            .CountAsync();

        var totalContentCount = await _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted && 
                      !string.IsNullOrEmpty(c.TagsJson) &&
                      c.TagsJson.ToLower().Contains(normalizedTag))
            .CountAsync();

        var isTrending = recentUsageCount >= 50;

        var dto = new HashtagDetailDto
        {
            Tag = normalizedTag,
            UsageCount = recentUsageCount,
            ContentCount = totalContentCount,
            Trending = isTrending
        };

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15));

        return BaseResponse<HashtagDetailDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<PagedResponse<ContentDto>>> GetContentsByTagAsync(
        string tag,
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();
        var normalizedTag = tag.ToLower().TrimStart('#');

        var cacheKey = $"Tag:{normalizedTag}:Contents:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
        }

        var query = _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted && 
                      !string.IsNullOrEmpty(c.TagsJson) &&
                      c.TagsJson.ToLower().Contains(normalizedTag))
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .OrderByDescending(c => c.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var contents = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var content in contents)
        {
            var dto = await MapToContentDtoAsync(content, userId);
            dtos.Add(dto);
        }

        var response = new PagedResponse<ContentDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<List<HashtagDto>>> SearchHashtagsAsync(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BaseResponse<List<HashtagDto>>.ErrorResponse("Query required", ErrorCodes.ValidationFailed);

        var queryLower = query.ToLower().Trim();

        var allTags = await _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted && !string.IsNullOrEmpty(c.TagsJson))
            .Select(c => c.TagsJson)
            .ToListAsync();

        var uniqueTags = new HashSet<string>();
        foreach (var tagsJson in allTags)
        {
            if (string.IsNullOrEmpty(tagsJson)) continue;
            var tags = JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>();
            foreach (var tag in tags)
            {
                var normalizedTag = tag.ToLower().TrimStart('#');
                if (normalizedTag.Contains(queryLower))
                {
                    uniqueTags.Add(normalizedTag);
                }
            }
        }

        var results = uniqueTags
            .Take(limit)
            .Select(tag => new HashtagDto
            {
                Tag = tag,
                UsageCount = 0
            })
            .ToList();

        return BaseResponse<List<HashtagDto>>.SuccessResponse(results);
    }

    // ========== SEARCH & DISCOVERY ==========

    public async Task<BaseResponse<PagedResponse<ContentDto>>> SearchContentsAsync(
        string? query,
        int? lessonId,
        int? topicId,
        DifficultyLevel? difficulty,
        ContentType? type,
        string sortBy = "popular",
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"Search:Contents:Q{query}:L{lessonId}:T{topicId}:D{difficulty}:Type{type}:Sort{sortBy}:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(cached);
        }

        var efQuery = _context.Contents
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            efQuery = efQuery.Where(c => 
                c.Title.Contains(query) || 
                c.Description != null && c.Description.Contains(query) ||
                !string.IsNullOrEmpty(c.TagsJson) && c.TagsJson.Contains(query));
        }

        if (lessonId.HasValue)
            efQuery = efQuery.Where(c => c.LessonId == lessonId);

        if (topicId.HasValue)
            efQuery = efQuery.Where(c => c.TopicId == topicId);

        if (difficulty.HasValue)
            efQuery = efQuery.Where(c => c.Difficulty == difficulty);

        if (type.HasValue)
            efQuery = efQuery.Where(c => c.ContentType == type);

        efQuery = sortBy switch
        {
            "recent" => efQuery.OrderByDescending(c => c.CreatedAt),
            "trending" => efQuery.OrderByDescending(c => 
                c.LikesCount * 2 + c.CommentCount * 3 + 
                (DateTime.UtcNow - c.CreatedAt).TotalHours < 24 ? 10 : 0),
            _ => efQuery.OrderByDescending(c => c.LikesCount + c.CommentCount * 2)
        };

        var totalCount = await efQuery.CountAsync();

        var contentsList = await efQuery
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var content in contentsList)
        {
            var dto = await MapToContentDtoAsync(content, userId);
            dtos.Add(dto);
        }

        var response = new PagedResponse<ContentDto>(dtos, totalCount, page, limit);
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        return BaseResponse<PagedResponse<ContentDto>>.SuccessResponse(response);
    }

    // ========== CONTENT ANALYTICS ==========

    public async Task<BaseResponse<ContentAnalyticsDto>> GetContentAnalyticsAsync(
        int contentId,
        string period = "week")
    {
        // 1. YETKİ KONTROLÜ - Herkes yapabilir (içerik sahibi kontrolü iş mantığında)
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.AdminAdmin,
            UserRole.Admin,
            UserRole.Manager,
            UserRole.Teacher,
            UserRole.StandaloneTeacher,
            UserRole.Student,
            UserRole.StandaloneStudent);
        if (authError != null)
            return BaseResponse<ContentAnalyticsDto>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contentId);

        if (content == null)
            return BaseResponse<ContentAnalyticsDto>.ErrorResponse(
                "Content not found", ErrorCodes.NotFound);

        // İçerik sahibi kontrolü
        if (content.AuthorId != userId)
        {
            // Admin analytics görebilir
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || (user.GlobalRole != UserRole.AdminAdmin && user.GlobalRole != UserRole.Admin))
                return BaseResponse<ContentAnalyticsDto>.ErrorResponse(
                    "Bu içeriğin analitiğini görme yetkiniz yok", ErrorCodes.AccessDenied);
        }

        var cacheKey = $"Content:Analytics:{contentId}:{period}";
        var cached = await _cacheService.GetAsync<ContentAnalyticsDto>(cacheKey);
        if (cached != null)
            return BaseResponse<ContentAnalyticsDto>.SuccessResponse(cached);

        var startDate = period switch
        {
            "day" => DateTime.UtcNow.AddDays(-1),
            "week" => DateTime.UtcNow.AddDays(-7),
            "month" => DateTime.UtcNow.AddDays(-30),
            _ => DateTime.MinValue
        };

        var views = await _context.Interactions
            .AsNoTracking()
            .Where(i => i.ContentId == contentId && 
                       i.Type == InteractionType.View &&
                       i.CreatedAt >= startDate)
            .CountAsync();

        var likes = await _context.Interactions
            .AsNoTracking()
            .Where(i => i.ContentId == contentId && 
                       i.Type == InteractionType.Like &&
                       i.CreatedAt >= startDate)
            .CountAsync();

        var comments = await _context.Comments
            .AsNoTracking()
            .Where(c => c.ContentId == contentId && 
                       !c.IsDeleted &&
                       c.CreatedAt >= startDate)
            .CountAsync();

        var saves = await _context.Interactions
            .AsNoTracking()
            .Where(i => i.ContentId == contentId && 
                       i.Type == InteractionType.Save &&
                       i.CreatedAt >= startDate)
            .CountAsync();

        var shares = await _context.Interactions
            .AsNoTracking()
            .Where(i => i.ContentId == contentId && 
                       i.Type == InteractionType.Share &&
                       i.CreatedAt >= startDate)
            .CountAsync();

        var engagementRate = views > 0 
            ? ((likes + comments + saves + shares) / (double)views) * 100 
            : 0;

        var viewsByDay = await _context.Interactions
            .AsNoTracking()
            .Where(i => i.ContentId == contentId && 
                       i.Type == InteractionType.View &&
                       i.CreatedAt >= startDate)
            .GroupBy(i => i.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Views = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var topEngagers = await _context.Interactions
            .AsNoTracking()
            .Where(i => i.ContentId == contentId && 
                       i.CreatedAt >= startDate)
            .GroupBy(i => i.UserId)
            .Select(g => new { 
                UserId = g.Key, 
                Interactions = g.Count() 
            })
            .OrderByDescending(x => x.Interactions)
            .Take(10)
            .Join(_context.Users,
                e => e.UserId,
                u => u.Id,
                (e, u) => new { 
                    UserId = u.Id, 
                    Username = u.Username, 
                    Interactions = e.Interactions 
                })
            .ToListAsync();

        var analytics = new ContentAnalyticsDto
        {
            ContentId = contentId,
            Views = views,
            Likes = likes,
            Comments = comments,
            Saves = saves,
            Shares = shares,
            EngagementRate = Math.Round(engagementRate, 2),
            ViewsByDay = viewsByDay.Select(v => new DailyViewDto
            {
                Date = v.Date.ToString("yyyy-MM-dd"),
                Views = v.Views
            }).ToList(),
            TopEngagers = topEngagers.Select(e => new TopEngagerDto
            {
                UserId = e.UserId,
                Username = e.Username,
                Interactions = e.Interactions
            }).ToList()
        };

        await _cacheService.SetAsync(cacheKey, analytics, TimeSpan.FromMinutes(10));
        return BaseResponse<ContentAnalyticsDto>.SuccessResponse(analytics);
    }

    // ========== CONTENT MODERATION ==========

    public async Task<BaseResponse<string>> ReportContentAsync(
        int contentId,
        ReportContentRequest request)
    {
        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .FirstOrDefaultAsync(c => c.Id == contentId);

        if (content == null)
            return BaseResponse<string>.ErrorResponse(
                "Content not found", ErrorCodes.NotFound);

        var existingReport = await _context.ContentReports
            .FirstOrDefaultAsync(r => r.ContentId == contentId && r.UserId == userId);

        if (existingReport != null)
            return BaseResponse<string>.ErrorResponse(
                "You have already reported this content", ErrorCodes.ValidationFailed);

        var report = new ContentReport
        {
            ContentId = contentId,
            UserId = userId,
            Reason = request.Reason,
            Description = request.Description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.ContentReports.Add(report);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(userId, "ContentReported", 
            $"Content {contentId} reported: {request.Reason}");

        await _cacheService.RemoveByPatternAsync("Admin:ContentReports:*");

        return BaseResponse<string>.SuccessResponse("Content reported successfully");
    }

    public async Task<BaseResponse<PagedResponse<ContentReportDto>>> GetContentReportsAsync(
        string? status,
        int page = 1,
        int limit = 20,
        bool forceRefresh = false)
    {
        // 1. YETKİ KONTROLÜ
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.AdminAdmin,
            UserRole.Admin);
        if (authError != null)
            return BaseResponse<PagedResponse<ContentReportDto>>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        var adminId = _sessionService.GetUserId();

        var cacheKey = $"Admin:ContentReports:Status{status}:Page{page}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PagedResponse<ContentReportDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<PagedResponse<ContentReportDto>>.SuccessResponse(cached);
        }

        var queryable = _context.ContentReports
            .AsNoTracking()
            .Include(r => r.Content)
            .Include(r => r.User)
            .Include(r => r.Reviewer)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<ReportStatus>(status, true);
            queryable = queryable.Where(r => r.Status == statusEnum);
        }

        var totalCount = await queryable.CountAsync();

        var reports = await queryable
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var dtos = reports.Select(r => new ContentReportDto
        {
            Id = r.Id,
            ContentId = r.ContentId,
            ContentTitle = r.Content.Title,
            ReporterId = r.UserId,
            ReporterName = r.User.FullName,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.Status,
            ReviewedById = r.ReviewedBy,
            ReviewerName = r.Reviewer != null ? r.Reviewer.FullName : null,
            ReviewedAt = r.ReviewedAt,
            ResolutionNotes = r.ReviewNotes,
            CreatedAt = r.CreatedAt
        }).ToList();

        var response = new PagedResponse<ContentReportDto>(dtos, totalCount, page, limit);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
        return BaseResponse<PagedResponse<ContentReportDto>>.SuccessResponse(response);
    }

    public async Task<BaseResponse<string>> ReviewContentReportAsync(
        int reportId,
        ReviewReportRequest request)
    {
        // 1. YETKİ KONTROLÜ
        var authError = _authorizationService.RequireGlobalRole(
            UserRole.AdminAdmin,
            UserRole.Admin);
        if (authError != null)
            return BaseResponse<string>.ErrorResponse(
                authError.Error ?? "Yetkiniz yok",
                authError.ErrorCode ?? ErrorCodes.AccessDenied);

        var adminId = _sessionService.GetUserId();

        var report = await _context.ContentReports
            .Include(r => r.Content)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
            return BaseResponse<string>.ErrorResponse(
                "Report not found", ErrorCodes.NotFound);

        if (report.Status != ReportStatus.Pending)
            return BaseResponse<string>.ErrorResponse(
                "Report already reviewed", ErrorCodes.ValidationFailed);

        report.Status = request.Action == "resolve" 
            ? ReportStatus.Resolved 
            : ReportStatus.Rejected;
        report.ReviewedBy = adminId;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewNotes = request.Notes;

        if (request.Action == "resolve")
        {
            report.Content.IsDeleted = true;
            report.Content.DeletedAt = DateTime.UtcNow;

            await _notificationService.SendNotificationAsync(
                report.Content.AuthorId,
                "İçerik Kaldırıldı",
                "İçeriğiniz bir şikayet nedeniyle kaldırıldı",
                NotificationType.System,
                $"/content/{report.ContentId}");
        }

        await _context.SaveChangesAsync();

        await _cacheService.RemoveByPatternAsync($"Content:{report.ContentId}:*");
        await _cacheService.RemoveByPatternAsync("Admin:ContentReports:*");

        await _auditService.LogAsync(adminId, "ContentReportReviewed", 
            $"Report {reportId} reviewed: {request.Action}");

        return BaseResponse<string>.SuccessResponse("Report reviewed successfully");
    }

    // ========== CONTENT EXPORT & SHARING ==========

    public async Task<BaseResponse<ShareLinkDto>> GetShareLinkAsync(int contentId)
    {
        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contentId);

        if (content == null)
            return BaseResponse<ShareLinkDto>.ErrorResponse(
                "Content not found", ErrorCodes.NotFound);

        // Basit token oluşturma (production'da JWT kullanılmalı)
        var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{contentId}:{userId}:{DateTime.UtcNow.Ticks}"));

        var shareLink = new ShareLinkDto
        {
            ShareLink = $"https://karneapp.com/share/content/{contentId}?token={token}",
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        return BaseResponse<ShareLinkDto>.SuccessResponse(shareLink);
    }

    public async Task<BaseResponse<ContentDto>> GetSharedContentAsync(
        int contentId,
        string token)
    {
        // Basit token doğrulama (production'da JWT kullanılmalı)
        if (string.IsNullOrEmpty(token))
            return BaseResponse<ContentDto>.ErrorResponse(
                "Invalid or expired share token", ErrorCodes.Unauthorized);

        var content = await _context.Contents
            .AsNoTracking()
            .Where(c => c.Id == contentId && !c.IsDeleted)
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .FirstOrDefaultAsync();

        if (content == null)
            return BaseResponse<ContentDto>.ErrorResponse(
                "Content not found", ErrorCodes.NotFound);

        var dto = await MapToContentDtoAsync(content, 0); // Public view, no user context
        return BaseResponse<ContentDto>.SuccessResponse(dto);
    }

    // ========== MUTE SYSTEM (EXTENDED) ==========

    public async Task<BaseResponse<List<UserProfileSocialDto>>> GetMutedUsersAsync()
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{userId}:MutedUsers";
        var cached = await _cacheService.GetAsync<List<UserProfileSocialDto>>(cacheKey);
        if (cached != null)
            return BaseResponse<List<UserProfileSocialDto>>.SuccessResponse(cached);

        var mutes = await _context.Mutes
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Include(m => m.MutedUser)
            .ToListAsync();

        var dtos = new List<UserProfileSocialDto>();
        foreach (var mute in mutes)
        {
            var contentCount = await _context.Contents
                .AsNoTracking()
                .CountAsync(c => c.AuthorId == mute.MutedUserId && !c.IsDeleted);

            dtos.Add(new UserProfileSocialDto
            {
                UserId = mute.MutedUserId,
                Username = mute.MutedUser.Username,
                FullName = mute.MutedUser.FullName,
                ProfileImageUrl = mute.MutedUser.ProfileImageUrl,
                FollowerCount = mute.MutedUser.FollowerCount,
                FollowingCount = mute.MutedUser.FollowingCount,
                ContentCount = contentCount,
                IsFollowing = false,
                IsBlocked = false,
                IsMuted = true
            });
        }

        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10));

        return BaseResponse<List<UserProfileSocialDto>>.SuccessResponse(dtos);
    }

    // ========== STORIES (EXTENDED) ==========

    public async Task<BaseResponse<List<StoryDto>>> GetUserStoriesAsync(
        int targetUserId,
        bool forceRefresh = false)
    {
        var currentUserId = _sessionService.GetUserId();

        if (targetUserId != currentUserId)
        {
            var isFollowing = await _context.Follows
                .AsNoTracking()
                .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);

            if (!isFollowing)
            {
                return BaseResponse<List<StoryDto>>.ErrorResponse(
                    "You must follow this user to view their stories", 
                    ErrorCodes.AccessDenied);
            }
        }

        var cacheKey = $"User:{targetUserId}:Stories";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<StoryDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<List<StoryDto>>.SuccessResponse(cached);
        }

        var now = DateTime.UtcNow;
        var stories = await _context.Stories
            .AsNoTracking()
            .Where(s => s.AuthorId == targetUserId && 
                       !s.IsDeleted &&
                       s.ExpiresAt > now)
            .Include(s => s.Author)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var dtos = stories.Select(s => MapToStoryDto(s, currentUserId)).ToList();

        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(1));

        return BaseResponse<List<StoryDto>>.SuccessResponse(dtos);
    }

    // ========== POLLS (ANKETLER) ==========

    public async Task<BaseResponse<PollDto>> CreatePollAsync(CreatePollRequest request)
    {
        var userId = _sessionService.GetUserId();

        // Validation
        if (string.IsNullOrWhiteSpace(request.Question))
            return BaseResponse<PollDto>.ErrorResponse("Question is required", ErrorCodes.ValidationFailed);

        if (request.Options == null || request.Options.Count < 2)
            return BaseResponse<PollDto>.ErrorResponse("At least 2 options are required", ErrorCodes.ValidationFailed);

        if (request.Options.Count > 10)
            return BaseResponse<PollDto>.ErrorResponse("Maximum 10 options allowed", ErrorCodes.ValidationFailed);

        // Content kontrolü
        var content = await _context.Contents
            .FirstOrDefaultAsync(c => c.Id == request.ContentId && !c.IsDeleted);

        if (content == null)
            return BaseResponse<PollDto>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        if (content.AuthorId != userId)
            return BaseResponse<PollDto>.ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

        // Aynı content'te zaten poll var mı?
        var existingPoll = await _context.Polls
            .FirstOrDefaultAsync(p => p.ContentId == request.ContentId);

        if (existingPoll != null)
            return BaseResponse<PollDto>.ErrorResponse("Content already has a poll", ErrorCodes.ValidationFailed);

        var expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(7);

        var poll = new Poll
        {
            ContentId = request.ContentId,
            Question = request.Question,
            OptionsJson = JsonSerializer.Serialize(request.Options),
            ExpiresAt = expiresAt,
            IsMultipleChoice = request.IsMultipleChoice,
            IsAnonymous = request.IsAnonymous,
            CreatedAt = DateTime.UtcNow
        };

        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Poll:{poll.Id}:*");
        await _cacheService.RemoveByPatternAsync($"Content:{request.ContentId}:*");

        // Audit log
        await _auditService.LogAsync(userId, "PollCreated", 
            JsonSerializer.Serialize(new { PollId = poll.Id, ContentId = request.ContentId }));

        var dto = await MapToPollDtoAsync(poll, userId);
        return BaseResponse<PollDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<bool>> VotePollAsync(int pollId, VotePollRequest request)
    {
        var userId = _sessionService.GetUserId();

        var poll = await _context.Polls
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (poll == null)
            return BaseResponse<bool>.ErrorResponse("Poll not found", ErrorCodes.NotFound);

        // Expire kontrolü
        if (poll.ExpiresAt <= DateTime.UtcNow)
            return BaseResponse<bool>.ErrorResponse("Poll has expired", ErrorCodes.ValidationFailed);

        // Validation
        if (request.OptionIndices == null || request.OptionIndices.Count == 0)
            return BaseResponse<bool>.ErrorResponse("At least one option must be selected", ErrorCodes.ValidationFailed);

        var options = JsonSerializer.Deserialize<List<string>>(poll.OptionsJson) ?? new List<string>();
        if (request.OptionIndices.Any(idx => idx < 0 || idx >= options.Count))
            return BaseResponse<bool>.ErrorResponse("Invalid option index", ErrorCodes.ValidationFailed);

        // Multiple choice kontrolü
        if (!poll.IsMultipleChoice && request.OptionIndices.Count > 1)
            return BaseResponse<bool>.ErrorResponse("Multiple choice is not allowed for this poll", ErrorCodes.ValidationFailed);

        // Kullanıcı daha önce oy vermiş mi? (Multiple choice ise tüm oyları kontrol et)
        var existingVotes = await _context.PollVotes
            .Where(v => v.PollId == pollId && v.UserId == userId)
            .ToListAsync();

        if (existingVotes.Any())
        {
            // Mevcut oyları sil (yeniden oy verme)
            _context.PollVotes.RemoveRange(existingVotes);
            poll.TotalVotes -= existingVotes.Count;
        }

        // Yeni oyları ekle
        foreach (var optionIndex in request.OptionIndices)
        {
            // Aynı seçeneği birden fazla kez seçemez
            if (existingVotes.Any(v => v.OptionIndex == optionIndex))
                continue;

            var vote = new PollVote
            {
                PollId = pollId,
                UserId = userId,
                OptionIndex = optionIndex,
                CreatedAt = DateTime.UtcNow
            };
            _context.PollVotes.Add(vote);
            poll.TotalVotes++;
        }

        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"Poll:{pollId}:*");
        await _cacheService.RemoveByPatternAsync($"Content:{poll.ContentId}:*");

        // SignalR: Real-time poll update
        await _notificationHub.Clients.Group($"Content_{poll.ContentId}")
            .SendAsync("PollVoted", new { PollId = pollId, TotalVotes = poll.TotalVotes });

        // Audit log
        await _auditService.LogAsync(userId, "PollVoted", 
            JsonSerializer.Serialize(new { PollId = pollId, Options = request.OptionIndices }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<PollDto>> GetPollAsync(int pollId, bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"Poll:{pollId}:User:{userId}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PollDto>(cacheKey);
            if (cached != null)
                return BaseResponse<PollDto>.SuccessResponse(cached);
        }

        var poll = await _context.Polls
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (poll == null)
            return BaseResponse<PollDto>.ErrorResponse("Poll not found", ErrorCodes.NotFound);

        // Anonymous değilse kullanıcının oylarını dahil et
        if (!poll.IsAnonymous)
        {
            poll = await _context.Polls
                .AsNoTracking()
                .Include(p => p.Votes.Where(v => v.UserId == userId))
                .FirstOrDefaultAsync(p => p.Id == pollId);
        }

        if (poll == null)
            return BaseResponse<PollDto>.ErrorResponse("Poll not found", ErrorCodes.NotFound);

        var dto = await MapToPollDtoAsync(poll, userId);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

        return BaseResponse<PollDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<PollResultDto>> GetPollResultsAsync(int pollId, bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var poll = await _context.Polls
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (poll == null)
            return BaseResponse<PollResultDto>.ErrorResponse("Poll not found", ErrorCodes.NotFound);

        // Content sahibi veya admin mi?
        var content = await _context.Contents
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == poll.ContentId);

        if (content == null)
            return BaseResponse<PollResultDto>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        if (content.AuthorId != userId)
        {
            // Admin kontrolü (opsiyonel)
            // if (!await IsAdminAsync(userId))
            //     return BaseResponse<PollResultDto>.ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
        }

        var cacheKey = $"Poll:{pollId}:Results";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<PollResultDto>(cacheKey);
            if (cached != null)
                return BaseResponse<PollResultDto>.SuccessResponse(cached);
        }

        var options = JsonSerializer.Deserialize<List<string>>(poll.OptionsJson) ?? new List<string>();
        var votes = await _context.PollVotes
            .AsNoTracking()
            .Where(v => v.PollId == pollId)
            .GroupBy(v => v.OptionIndex)
            .Select(g => new { OptionIndex = g.Key, VoteCount = g.Count() })
            .ToListAsync();

        var totalVotes = poll.TotalVotes;
        var optionResults = options.Select((option, index) =>
        {
            var voteData = votes.FirstOrDefault(v => v.OptionIndex == index);
            var voteCount = voteData?.VoteCount ?? 0;
            var percentage = totalVotes > 0 ? (voteCount / (double)totalVotes) * 100 : 0;

            return new PollOptionResultDto
            {
                Index = index,
                Text = option,
                VoteCount = voteCount,
                Percentage = Math.Round(percentage, 2)
            };
        }).ToList();

        var result = new PollResultDto
        {
            PollId = pollId,
            Question = poll.Question,
            Options = optionResults,
            TotalVotes = totalVotes,
            IsExpired = poll.ExpiresAt <= DateTime.UtcNow,
            ExpiresAt = poll.ExpiresAt
        };

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return BaseResponse<PollResultDto>.SuccessResponse(result);
    }

    private async Task<PollDto> MapToPollDtoAsync(Poll poll, int userId)
    {
        var options = JsonSerializer.Deserialize<List<string>>(poll.OptionsJson) ?? new List<string>();
        
        // Kullanıcının oylarını al (anonymous değilse)
        List<int>? userVotes = null;
        bool hasVoted = false;
        if (!poll.IsAnonymous)
        {
            var userVoteList = await _context.PollVotes
                .AsNoTracking()
                .Where(v => v.PollId == poll.Id && v.UserId == userId)
                .Select(v => v.OptionIndex)
                .ToListAsync();

            if (userVoteList.Any())
            {
                hasVoted = true;
                userVotes = userVoteList;
            }
        }
        else
        {
            // Anonymous ise sadece oy verip vermediğini kontrol et
            hasVoted = await _context.PollVotes
                .AsNoTracking()
                .AnyAsync(v => v.PollId == poll.Id && v.UserId == userId);
        }

        return new PollDto
        {
            Id = poll.Id,
            ContentId = poll.ContentId,
            Question = poll.Question,
            Options = options,
            ExpiresAt = poll.ExpiresAt,
            IsMultipleChoice = poll.IsMultipleChoice,
            IsAnonymous = poll.IsAnonymous,
            TotalVotes = poll.TotalVotes,
            HasVoted = hasVoted,
            UserVotes = userVotes,
            CreatedAt = poll.CreatedAt
        };
    }

    // ========== DRAFTS (TASLAKLAR) ==========

    public async Task<BaseResponse<DraftDto>> SaveDraftAsync(SaveDraftRequest request, int? draftId = null)
    {
        var userId = _sessionService.GetUserId();

        // Validation
        if (string.IsNullOrWhiteSpace(request.Title))
            return BaseResponse<DraftDto>.ErrorResponse("Title is required", ErrorCodes.ValidationFailed);

        ContentDraft? draft;

        if (draftId.HasValue)
        {
            // Mevcut draft'i güncelle
            draft = await _context.ContentDrafts
                .FirstOrDefaultAsync(d => d.Id == draftId.Value && d.AuthorId == userId);

            if (draft == null)
                return BaseResponse<DraftDto>.ErrorResponse("Draft not found", ErrorCodes.NotFound);

            draft.ContentType = request.ContentType;
            draft.Title = request.Title;
            draft.Description = request.Description;
            draft.ImageUrl = request.ImageUrl;
            draft.VideoUrl = request.VideoUrl;
            draft.FileUrl = request.FileUrl;
            draft.LessonId = request.LessonId;
            draft.TopicId = request.TopicId;
            draft.Difficulty = request.Difficulty;
            draft.TagsJson = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null;
            draft.LastSavedAt = DateTime.UtcNow;
        }
        else
        {
            // Yeni draft oluştur
            draft = new ContentDraft
            {
                AuthorId = userId,
                ContentType = request.ContentType,
                Title = request.Title,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                VideoUrl = request.VideoUrl,
                FileUrl = request.FileUrl,
                LessonId = request.LessonId,
                TopicId = request.TopicId,
                Difficulty = request.Difficulty,
                TagsJson = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
                CreatedAt = DateTime.UtcNow,
                LastSavedAt = DateTime.UtcNow
            };
            _context.ContentDrafts.Add(draft);
        }

        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Drafts:*");

        // Audit log
        await _auditService.LogAsync(userId, draftId.HasValue ? "DraftUpdated" : "DraftCreated", 
            JsonSerializer.Serialize(new { DraftId = draft!.Id }));

        var dto = await MapToDraftDtoAsync(draft);
        return BaseResponse<DraftDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<List<DraftDto>>> GetDraftsAsync(bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"User:{userId}:Drafts";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<DraftDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<List<DraftDto>>.SuccessResponse(cached);
        }

        var drafts = await _context.ContentDrafts
            .AsNoTracking()
            .Where(d => d.AuthorId == userId)
            .OrderByDescending(d => d.LastSavedAt)
            .ToListAsync();

        var dtos = new List<DraftDto>();
        foreach (var draft in drafts)
        {
            dtos.Add(await MapToDraftDtoAsync(draft));
        }

        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10));
        return BaseResponse<List<DraftDto>>.SuccessResponse(dtos);
    }

    public async Task<BaseResponse<DraftDto>> GetDraftAsync(int draftId, bool forceRefresh = false)
    {
        var userId = _sessionService.GetUserId();

        var cacheKey = $"Draft:{draftId}";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<DraftDto>(cacheKey);
            if (cached != null)
                return BaseResponse<DraftDto>.SuccessResponse(cached);
        }

        var draft = await _context.ContentDrafts
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == draftId && d.AuthorId == userId);

        if (draft == null)
            return BaseResponse<DraftDto>.ErrorResponse("Draft not found", ErrorCodes.NotFound);

        var dto = await MapToDraftDtoAsync(draft);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));

        return BaseResponse<DraftDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<ContentDto>> PublishDraftAsync(int draftId)
    {
        var userId = _sessionService.GetUserId();

        var draft = await _context.ContentDrafts
            .FirstOrDefaultAsync(d => d.Id == draftId && d.AuthorId == userId);

        if (draft == null)
            return BaseResponse<ContentDto>.ErrorResponse("Draft not found", ErrorCodes.NotFound);

        // Content oluştur
        var content = new Content
        {
            AuthorId = userId,
            ContentType = draft.ContentType,
            Title = draft.Title,
            Description = draft.Description,
            ImageUrl = draft.ImageUrl,
            VideoUrl = draft.VideoUrl,
            FileUrl = draft.FileUrl,
            LessonId = draft.LessonId,
            TopicId = draft.TopicId,
            Difficulty = draft.Difficulty,
            TagsJson = draft.TagsJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync();

        // Draft'i sil
        _context.ContentDrafts.Remove(draft);
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Drafts:*");
        await _cacheService.RemoveByPatternAsync($"Content:*");
        await _cacheService.RemoveByPatternAsync($"Feed:*");

        // SignalR: Takipçilere bildirim
        var followers = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowingId == userId)
            .Select(f => f.FollowerId)
            .ToListAsync();

        foreach (var followerId in followers)
        {
            await _notificationHub.Clients.Group($"User_{followerId}")
                .SendAsync("NewContent", new { AuthorId = userId, ContentId = content.Id, Title = content.Title });
        }

        // Hangfire: RediSearch index
        BackgroundJob.Enqueue(() => IndexContentAsync(content.Id));

        // Audit log
        await _auditService.LogAsync(userId, "DraftPublished", 
            JsonSerializer.Serialize(new { DraftId = draftId, ContentId = content.Id }));

        var dto = await MapToContentDtoAsync(content, userId);
        return BaseResponse<ContentDto>.SuccessResponse(dto);
    }

    public async Task<BaseResponse<bool>> DeleteDraftAsync(int draftId)
    {
        var userId = _sessionService.GetUserId();

        var draft = await _context.ContentDrafts
            .FirstOrDefaultAsync(d => d.Id == draftId && d.AuthorId == userId);

        if (draft == null)
            return BaseResponse<bool>.ErrorResponse("Draft not found", ErrorCodes.NotFound);

        _context.ContentDrafts.Remove(draft);
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Drafts:*");
        await _cacheService.RemoveByPatternAsync($"Draft:{draftId}:*");

        // Audit log
        await _auditService.LogAsync(userId, "DraftDeleted", 
            JsonSerializer.Serialize(new { DraftId = draftId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    private async Task<DraftDto> MapToDraftDtoAsync(ContentDraft draft)
    {
        var tags = !string.IsNullOrEmpty(draft.TagsJson)
            ? JsonSerializer.Deserialize<List<string>>(draft.TagsJson) ?? new List<string>()
            : new List<string>();

        var lessonName = draft.LessonId.HasValue
            ? await _context.Lessons
                .AsNoTracking()
                .Where(l => l.Id == draft.LessonId.Value)
                .Select(l => l.Name)
                .FirstOrDefaultAsync()
            : null;

        var topicName = draft.TopicId.HasValue
            ? await _context.Topics
                .AsNoTracking()
                .Where(t => t.Id == draft.TopicId.Value)
                .Select(t => t.Name)
                .FirstOrDefaultAsync()
            : null;

        return new DraftDto
        {
            Id = draft.Id,
            ContentType = draft.ContentType,
            Title = draft.Title,
            Description = draft.Description,
            ImageUrl = draft.ImageUrl,
            VideoUrl = draft.VideoUrl,
            FileUrl = draft.FileUrl,
            LessonId = draft.LessonId,
            LessonName = lessonName,
            TopicId = draft.TopicId,
            TopicName = topicName,
            Difficulty = draft.Difficulty,
            Tags = tags,
            LastSavedAt = draft.LastSavedAt,
            CreatedAt = draft.CreatedAt
        };
    }

    // ========== CONTENT PINNING (İÇERİK SABİTLEME) ==========

    public async Task<BaseResponse<bool>> PinContentAsync(int contentId)
    {
        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted);

        if (content == null)
            return BaseResponse<bool>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        if (content.AuthorId != userId)
            return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

        if (content.IsPinned)
            return BaseResponse<bool>.ErrorResponse("Content is already pinned", ErrorCodes.ValidationFailed);

        // Kullanıcının maksimum 3 içerik sabitleyebilir
        var pinnedCount = await _context.Contents
            .AsNoTracking()
            .CountAsync(c => c.AuthorId == userId && c.IsPinned && !c.IsDeleted);

        if (pinnedCount >= 3)
            return BaseResponse<bool>.ErrorResponse("Maximum 3 contents can be pinned", ErrorCodes.ValidationFailed);

        content.IsPinned = true;
        content.PinnedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Contents:*");
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Pinned:*");
        await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");

        // Audit log
        await _auditService.LogAsync(userId, "ContentPinned", 
            JsonSerializer.Serialize(new { ContentId = contentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<bool>> UnpinContentAsync(int contentId)
    {
        var userId = _sessionService.GetUserId();

        var content = await _context.Contents
            .FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted);

        if (content == null)
            return BaseResponse<bool>.ErrorResponse("Content not found", ErrorCodes.NotFound);

        if (content.AuthorId != userId)
            return BaseResponse<bool>.ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

        if (!content.IsPinned)
            return BaseResponse<bool>.ErrorResponse("Content is not pinned", ErrorCodes.ValidationFailed);

        content.IsPinned = false;
        content.PinnedAt = null;
        await _context.SaveChangesAsync();

        // Cache invalidation
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Contents:*");
        await _cacheService.RemoveByPatternAsync($"User:{userId}:Pinned:*");
        await _cacheService.RemoveByPatternAsync($"Content:{contentId}:*");

        // Audit log
        await _auditService.LogAsync(userId, "ContentUnpinned", 
            JsonSerializer.Serialize(new { ContentId = contentId }));

        return BaseResponse<bool>.SuccessResponse(true);
    }

    public async Task<BaseResponse<List<ContentDto>>> GetPinnedContentsAsync(int userId, bool forceRefresh = false)
    {
        var currentUserId = _sessionService.GetUserId();

        // Privacy kontrolü: Sadece kendi pinned içeriklerimizi veya public profillerin pinned içeriklerini görebiliriz
        if (userId != currentUserId)
        {
            var targetUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (targetUser == null)
                return BaseResponse<List<ContentDto>>.ErrorResponse("User not found", ErrorCodes.NotFound);

            if (targetUser.ProfileVisibility != Models.Enums.ProfileVisibility.PublicToAll)
            {
                // Takip ediyor muyuz?
                var isFollowing = await _context.Follows
                    .AsNoTracking()
                    .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == userId);

                if (!isFollowing)
                    return BaseResponse<List<ContentDto>>.ErrorResponse("Access denied", ErrorCodes.AccessDenied);
            }
        }

        var cacheKey = $"User:{userId}:Pinned";
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<List<ContentDto>>(cacheKey);
            if (cached != null)
                return BaseResponse<List<ContentDto>>.SuccessResponse(cached);
        }

        var contents = await _context.Contents
            .AsNoTracking()
            .Where(c => c.AuthorId == userId && c.IsPinned && !c.IsDeleted)
            .Include(c => c.Author)
            .Include(c => c.Lesson)
            .Include(c => c.Topic)
            .OrderByDescending(c => c.PinnedAt)
            .ToListAsync();

        var dtos = new List<ContentDto>();
        foreach (var content in contents)
        {
            var dto = await MapToContentDtoAsync(content, currentUserId);
            dtos.Add(dto);
        }

        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(15));
        return BaseResponse<List<ContentDto>>.SuccessResponse(dtos);
    }

    // Hangfire Jobs (Placeholder - RediSearch için)
    public static async Task IndexContentAsync(int contentId)
    {
        // RediSearch index güncelleme (eğer aktifse)
        // Şimdilik boş bırakıyoruz, RediSearch implement edildiğinde doldurulacak
        await Task.CompletedTask;
    }

    public static async Task DeleteContentIndexAsync(int contentId)
    {
        // RediSearch index'ten silme (eğer aktifse)
        await Task.CompletedTask;
    }
}

