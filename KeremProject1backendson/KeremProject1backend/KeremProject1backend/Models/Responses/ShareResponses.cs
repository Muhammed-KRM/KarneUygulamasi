namespace KeremProject1backend.Models.Responses
{
    public class ShareLinkResponse
    {
        public string ShareUrl { get; set; } = string.Empty;
        public string ShareToken { get; set; } = string.Empty;
    }

    public class PublicListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsLiked { get; set; }
        public string? ListImageLink { get; set; } // Liste görseli
    }
}

