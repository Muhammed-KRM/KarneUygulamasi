using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Models.DTOs.Responses;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Operations;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[ApiController]
[Route("api/social")]
[Authorize]
public class SocialController : BaseController
{
    private readonly SocialOperations _socialOperations;

    public SocialController(SocialOperations socialOperations, SessionService sessionService) : base(sessionService)
    {
        _socialOperations = socialOperations;
    }

    // ========== CONTENT MANAGEMENT ==========

    [HttpPost("content/create")]
    public async Task<IActionResult> CreateContent([FromBody] CreateContentRequest request)
    {
        var result = await _socialOperations.CreateContentAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("content/{id}")]
    public async Task<IActionResult> GetContentById(
        int id, 
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetContentByIdAsync(id, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("content/{id}")]
    public async Task<IActionResult> UpdateContent(
        int id, 
        [FromBody] UpdateContentRequest request)
    {
        var result = await _socialOperations.UpdateContentAsync(id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("content/{id}")]
    public async Task<IActionResult> DeleteContent(int id)
    {
        var result = await _socialOperations.DeleteContentAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== LIKE/UNLIKE ==========

    [HttpPost("content/{id}/like")]
    public async Task<IActionResult> LikeContent(int id)
    {
        var result = await _socialOperations.LikeContentAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("content/{id}/like")]
    public async Task<IActionResult> UnlikeContent(int id)
    {
        var result = await _socialOperations.UnlikeContentAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== COMMENT MANAGEMENT ==========

    [HttpPost("content/{id}/comment")]
    public async Task<IActionResult> CreateComment(
        int id, 
        [FromBody] CreateCommentRequest request)
    {
        var result = await _socialOperations.CreateCommentAsync(id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("content/{id}/comments")]
    public async Task<IActionResult> GetContentComments(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetContentCommentsAsync(id, page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("comment/{id}")]
    public async Task<IActionResult> UpdateComment(
        int id, 
        [FromBody] UpdateCommentRequest request)
    {
        var result = await _socialOperations.UpdateCommentAsync(id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("comment/{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var result = await _socialOperations.DeleteCommentAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== FOLLOW/UNFOLLOW ==========

    [HttpPost("user/{userId}/follow")]
    public async Task<IActionResult> FollowUser(int userId)
    {
        var result = await _socialOperations.FollowUserAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("user/{userId}/follow")]
    public async Task<IActionResult> UnfollowUser(int userId)
    {
        var result = await _socialOperations.UnfollowUserAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== SAVE/UNSAVE ==========

    [HttpPost("content/{id}/save")]
    public async Task<IActionResult> SaveContent(int id)
    {
        var result = await _socialOperations.SaveContentAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("content/{id}/save")]
    public async Task<IActionResult> UnsaveContent(int id)
    {
        var result = await _socialOperations.UnsaveContentAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== SHARE ==========

    [HttpPost("content/{id}/share")]
    public async Task<IActionResult> ShareContent(int id)
    {
        var result = await _socialOperations.ShareContentAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== BLOCK/MUTE ==========

    [HttpPost("user/{userId}/block")]
    public async Task<IActionResult> BlockUser(int userId)
    {
        var result = await _socialOperations.BlockUserAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("user/{userId}/block")]
    public async Task<IActionResult> UnblockUser(int userId)
    {
        var result = await _socialOperations.UnblockUserAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("user/{userId}/mute")]
    public async Task<IActionResult> MuteUser(int userId)
    {
        var result = await _socialOperations.MuteUserAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("user/{userId}/mute")]
    public async Task<IActionResult> UnmuteUser(int userId)
    {
        var result = await _socialOperations.UnmuteUserAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== STORIES ==========

    [HttpPost("story/create")]
    public async Task<IActionResult> CreateStory([FromBody] CreateStoryRequest request)
    {
        var result = await _socialOperations.CreateStoryAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("stories")]
    public async Task<IActionResult> GetStories(
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetStoriesAsync(forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("story/{id}")]
    public async Task<IActionResult> GetStoryById(
        int id,
        [FromQuery] bool markAsViewed = true)
    {
        var result = await _socialOperations.GetStoryByIdAsync(id, markAsViewed);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("story/{id}")]
    public async Task<IActionResult> DeleteStory(int id)
    {
        var result = await _socialOperations.DeleteStoryAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("story/{id}/reaction")]
    public async Task<IActionResult> ReactToStory(
        int id, 
        [FromBody] ReactToStoryRequest request)
    {
        var result = await _socialOperations.ReactToStoryAsync(id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== FEED SYSTEM ==========

    [HttpGet("feed/following")]
    public async Task<IActionResult> GetFollowingFeed(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetFollowingFeedAsync(page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("feed/for-you")]
    public async Task<IActionResult> GetForYouFeed(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetForYouFeedAsync(page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("feed/trending")]
    public async Task<IActionResult> GetTrendingContents(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetTrendingContentsAsync(page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("feed/popular")]
    public async Task<IActionResult> GetPopularContents(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetPopularContentsAsync(page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("feed/saved")]
    public async Task<IActionResult> GetSavedContents(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetSavedContentsAsync(page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendations(
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetRecommendedContentsAsync(limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== USER CONTENTS ==========

    [HttpGet("user/{userId}/contents")]
    public async Task<IActionResult> GetUserContents(
        int userId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetUserContentsAsync(userId, page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("user/{userId}/profile-social")]
    public async Task<IActionResult> GetUserProfileSocial(int userId)
    {
        var result = await _socialOperations.GetUserProfileSocialAsync(userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("user/{userId}/followers")]
    public async Task<IActionResult> GetFollowers(
        int userId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetFollowersAsync(userId, page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("user/{userId}/following")]
    public async Task<IActionResult> GetFollowing(
        int userId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetFollowingAsync(userId, page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("user/muted")]
    public async Task<IActionResult> GetMutedUsers()
    {
        var result = await _socialOperations.GetMutedUsersAsync();
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("user/{userId}/stories")]
    public async Task<IActionResult> GetUserStories(
        int userId,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetUserStoriesAsync(userId, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== COMMENT REPLIES ==========

    [HttpGet("comment/{id}/replies")]
    public async Task<IActionResult> GetCommentReplies(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetCommentRepliesAsync(id, page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== HASHTAGS & TAGS ==========

    [HttpGet("hashtags/trending")]
    public async Task<IActionResult> GetTrendingHashtags([FromQuery] int limit = 20)
    {
        var result = await _socialOperations.GetTrendingHashtagsAsync(limit);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("hashtags/{tag}")]
    public async Task<IActionResult> GetHashtagDetail(string tag)
    {
        var result = await _socialOperations.GetHashtagDetailAsync(tag);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("hashtags/{tag}/contents")]
    public async Task<IActionResult> GetContentsByTag(
        string tag,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetContentsByTagAsync(tag, page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("hashtags/search")]
    public async Task<IActionResult> SearchHashtags(
        [FromQuery] string query,
        [FromQuery] int limit = 10)
    {
        var result = await _socialOperations.SearchHashtagsAsync(query, limit);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== SEARCH & DISCOVERY ==========

    [HttpGet("search/contents")]
    public async Task<IActionResult> SearchContents(
        [FromQuery] string? query,
        [FromQuery] int? lessonId,
        [FromQuery] int? topicId,
        [FromQuery] string? difficulty,
        [FromQuery] string? type,
        [FromQuery] string sortBy = "popular",
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        DifficultyLevel? difficultyEnum = null;
        if (!string.IsNullOrEmpty(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var diff))
            difficultyEnum = diff;

        ContentType? typeEnum = null;
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<ContentType>(type, true, out var contentType))
            typeEnum = contentType;

        var result = await _socialOperations.SearchContentsAsync(
            query, lessonId, topicId, difficultyEnum, typeEnum, sortBy, page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== CONTENT ANALYTICS ==========

    [HttpGet("content/{id}/analytics")]
    public async Task<IActionResult> GetContentAnalytics(
        int id,
        [FromQuery] string period = "week")
    {
        var result = await _socialOperations.GetContentAnalyticsAsync(id, period);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== CONTENT MODERATION ==========

    [HttpPost("content/{id}/report")]
    public async Task<IActionResult> ReportContent(
        int id,
        [FromBody] ReportContentRequest request)
    {
        var result = await _socialOperations.ReportContentAsync(id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("admin/content/reports")]
    public async Task<IActionResult> GetContentReports(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetContentReportsAsync(status, page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("admin/content/report/{id}/review")]
    public async Task<IActionResult> ReviewContentReport(
        int id,
        [FromBody] ReviewReportRequest request)
    {
        var result = await _socialOperations.ReviewContentReportAsync(id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== CONTENT EXPORT & SHARING ==========

    [HttpGet("content/{id}/share-link")]
    public async Task<IActionResult> GetShareLink(int id)
    {
        var result = await _socialOperations.GetShareLinkAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("share/content/{id}")]
    public async Task<IActionResult> GetSharedContent(
        int id,
        [FromQuery] string token)
    {
        var result = await _socialOperations.GetSharedContentAsync(id, token);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== POLLS (ANKETLER) ==========

    [HttpPost("poll/create")]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
    {
        var result = await _socialOperations.CreatePollAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("poll/{id}/vote")]
    public async Task<IActionResult> VotePoll(
        int id,
        [FromBody] VotePollRequest request)
    {
        var result = await _socialOperations.VotePollAsync(id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("poll/{id}")]
    public async Task<IActionResult> GetPoll(
        int id,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetPollAsync(id, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("poll/{id}/results")]
    public async Task<IActionResult> GetPollResults(
        int id,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetPollResultsAsync(id, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== DRAFTS (TASLAKLAR) ==========

    [HttpPost("draft/save")]
    public async Task<IActionResult> SaveDraft([FromBody] SaveDraftRequest request)
    {
        var result = await _socialOperations.SaveDraftAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("draft/{id}")]
    public async Task<IActionResult> UpdateDraft(
        int id,
        [FromBody] SaveDraftRequest request)
    {
        var result = await _socialOperations.SaveDraftAsync(request, id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("drafts")]
    public async Task<IActionResult> GetDrafts([FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetDraftsAsync(forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("draft/{id}")]
    public async Task<IActionResult> GetDraft(
        int id,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetDraftAsync(id, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("draft/{id}/publish")]
    public async Task<IActionResult> PublishDraft(int id)
    {
        var result = await _socialOperations.PublishDraftAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("draft/{id}")]
    public async Task<IActionResult> DeleteDraft(int id)
    {
        var result = await _socialOperations.DeleteDraftAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ========== CONTENT PINNING (İÇERİK SABİTLEME) ==========

    [HttpPost("content/{id}/pin")]
    public async Task<IActionResult> PinContent(int id)
    {
        var result = await _socialOperations.PinContentAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("content/{id}/unpin")]
    public async Task<IActionResult> UnpinContent(int id)
    {
        var result = await _socialOperations.UnpinContentAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("user/{userId}/pinned")]
    public async Task<IActionResult> GetPinnedContents(
        int userId,
        [FromQuery] bool forceRefresh = false)
    {
        var result = await _socialOperations.GetPinnedContentsAsync(userId, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

