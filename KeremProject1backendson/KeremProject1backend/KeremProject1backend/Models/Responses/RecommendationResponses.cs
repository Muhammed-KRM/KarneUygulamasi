namespace KeremProject1backend.Models.Responses
{
    public class RecommendationDto
    {
        public int MalId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double? Score { get; set; }
        public string Reason { get; set; } = string.Empty; // Öneri nedeni
        public int MatchCount { get; set; } // Kaç kullanıcı bunu izlemiş
    }

    public class TrendingListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public double TrendingScore { get; set; } // Trending hesaplama skoru
    }
}

