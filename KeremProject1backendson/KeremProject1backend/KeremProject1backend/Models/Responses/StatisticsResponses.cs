namespace KeremProject1backend.Models.Responses
{
    public class UserStatisticsDto
    {
        public int TotalAnimeWatched { get; set; }
        public double AverageScore { get; set; }
        public int TotalLists { get; set; }
        public int PublicLists { get; set; }
        public int TotalLikes { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalFollowing { get; set; }
        public Dictionary<int, int> ScoreDistribution { get; set; } = new Dictionary<int, int>(); // Score -> Count
        public Dictionary<int, int> YearDistribution { get; set; } = new Dictionary<int, int>(); // Year -> Count
        public Dictionary<string, int> GenreDistribution { get; set; } = new Dictionary<string, int>(); // Genre -> Count
    }
}

