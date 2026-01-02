namespace KeremProject1backend.Models.Responses
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserImageLink { get; set; }
        public string? MalUsername { get; set; }
        public int TotalLists { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalFollowing { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsOwnProfile { get; set; }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? RelatedListId { get; set; }
        public int? RelatedUserId { get; set; }
        public string? RelatedUsername { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TemplateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;
        public int UseCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

