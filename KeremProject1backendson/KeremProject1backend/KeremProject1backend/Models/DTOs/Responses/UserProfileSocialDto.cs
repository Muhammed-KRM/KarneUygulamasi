namespace KeremProject1backend.Models.DTOs.Responses;

public class UserProfileSocialDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int ContentCount { get; set; }
    public bool IsFollowing { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsMuted { get; set; }
}

