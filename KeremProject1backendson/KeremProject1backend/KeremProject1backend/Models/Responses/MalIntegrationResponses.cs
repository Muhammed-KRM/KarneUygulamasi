using System.Text.Json.Serialization;

namespace KeremProject1backend.Models.Responses
{
    public class MalAuthUrlResponse
    {
        public string AuthUrl { get; set; } = string.Empty;
        public string CodeVerifier { get; set; } = string.Empty; // Frontend'in bunu saklaması lazım
    }

    public class MalTokenResponse
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class MalAnimeListResponse
    {
        [JsonPropertyName("data")]
        public List<MalAnimeNode> Data { get; set; } = new List<MalAnimeNode>();
    }

    public class MalAnimeNode
    {
        [JsonPropertyName("node")]
        public MalAnime Node { get; set; } = new MalAnime();

        [JsonPropertyName("list_status")]
        public MalListStatus ListStatus { get; set; } = new MalListStatus();
    }

    public class MalAnime
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("main_picture")]
        public MalPicture MainPicture { get; set; } = new MalPicture();
    }

    public class MalPicture
    {
        [JsonPropertyName("medium")]
        public string Medium { get; set; } = string.Empty;

        [JsonPropertyName("large")]
        public string Large { get; set; } = string.Empty;
    }

    public class MalListStatus
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty; // "completed"

        [JsonPropertyName("score")]
        public int Score { get; set; } // 0-10
    }

    public class MalUserProfileDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    // load.json formatı için model
    // anime_title bazen string bazen sayı olarak gelebilir (örn: 86), bu yüzden JsonElement kullanıyoruz
    public class MalLoadJsonItem
    {
        [JsonPropertyName("anime_id")]
        public int AnimeId { get; set; }

        [JsonPropertyName("anime_title")]
        public System.Text.Json.JsonElement AnimeTitle { get; set; }

        [JsonPropertyName("anime_image_path")]
        public string AnimeImagePath { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; } // 1: Watching, 2: Completed, 3: On Hold, 4: Dropped, 6: Plan to Watch

        [JsonPropertyName("is_rewatching")]
        public int IsRewatching { get; set; }

        [JsonPropertyName("num_watched_episodes")]
        public int NumWatchedEpisodes { get; set; }

        /// <summary>
        /// anime_title'ı string'e çevirir (sayı veya string olabilir)
        /// </summary>
        public string GetAnimeTitle()
        {
            if (AnimeTitle.ValueKind == System.Text.Json.JsonValueKind.String)
                return AnimeTitle.GetString() ?? string.Empty;
            else if (AnimeTitle.ValueKind == System.Text.Json.JsonValueKind.Number)
                return AnimeTitle.GetInt32().ToString();
            else
                return string.Empty;
        }
    }

    // Detaylı load.json response modeli (tüm alanlar)
    public class MalAdvancedAnimeListResponse
    {
        public List<MalAdvancedAnimeItem> Data { get; set; } = new List<MalAdvancedAnimeItem>();
    }

    public class MalAdvancedAnimeItem
    {
        [JsonPropertyName("anime_airing_status")]
        public int? AnimeAiringStatus { get; set; }

        [JsonPropertyName("anime_end_date_string")]
        public string? AnimeEndDateString { get; set; }

        [JsonPropertyName("anime_id")]
        public int AnimeId { get; set; }

        [JsonPropertyName("anime_image_path")]
        public string AnimeImagePath { get; set; } = string.Empty;

        [JsonPropertyName("anime_licensors")]
        public string? AnimeLicensors { get; set; }

        [JsonPropertyName("anime_media_type_string")]
        public string? AnimeMediaTypeString { get; set; }

        [JsonPropertyName("anime_mpaa_rating_string")]
        public string? AnimeMpaaRatingString { get; set; }

        [JsonPropertyName("anime_num_episodes")]
        public int? AnimeNumEpisodes { get; set; }

        [JsonPropertyName("anime_popularity")]
        public int? AnimePopularity { get; set; }

        [JsonPropertyName("anime_score_diff")]
        public double? AnimeScoreDiff { get; set; }

        [JsonPropertyName("anime_score_val")]
        public double? AnimeScoreVal { get; set; }

        [JsonPropertyName("anime_season")]
        public string? AnimeSeason { get; set; }

        [JsonPropertyName("anime_start_date_string")]
        public string? AnimeStartDateString { get; set; }

        [JsonPropertyName("anime_studios")]
        public string? AnimeStudios { get; set; }

        [JsonPropertyName("anime_title")]
        public System.Text.Json.JsonElement AnimeTitle { get; set; }

        [JsonPropertyName("anime_title_eng")]
        public string? AnimeTitleEng { get; set; }

        [JsonPropertyName("anime_total_members")]
        public int? AnimeTotalMembers { get; set; }

        [JsonPropertyName("anime_total_scores")]
        public int? AnimeTotalScores { get; set; }

        [JsonPropertyName("anime_url")]
        public string? AnimeUrl { get; set; }

        [JsonPropertyName("created_at")]
        public long? CreatedAt { get; set; }

        [JsonPropertyName("days_string")]
        public string? DaysString { get; set; }

        [JsonPropertyName("demographics")]
        public List<MalGenreItem> Demographics { get; set; } = new List<MalGenreItem>();

        [JsonPropertyName("editable_notes")]
        public string? EditableNotes { get; set; }

        [JsonPropertyName("finish_date_string")]
        public string? FinishDateString { get; set; }

        [JsonPropertyName("genres")]
        public List<MalGenreItem> Genres { get; set; } = new List<MalGenreItem>();

        [JsonPropertyName("has_episode_video")]
        public bool? HasEpisodeVideo { get; set; }

        [JsonPropertyName("has_promotion_video")]
        public bool? HasPromotionVideo { get; set; }

        [JsonPropertyName("has_video")]
        public bool? HasVideo { get; set; }

        [JsonPropertyName("is_added_to_list")]
        public bool? IsAddedToList { get; set; }

        [JsonPropertyName("is_rewatching")]
        public int IsRewatching { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("num_watched_episodes")]
        public int NumWatchedEpisodes { get; set; }

        [JsonPropertyName("priority_string")]
        public string? PriorityString { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("start_date_string")]
        public string? StartDateString { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("storage_string")]
        public string? StorageString { get; set; }

        [JsonPropertyName("tags")]
        public string? Tags { get; set; }

        [JsonPropertyName("title_localized")]
        public string? TitleLocalized { get; set; }

        [JsonPropertyName("updated_at")]
        public long? UpdatedAt { get; set; }

        [JsonPropertyName("video_url")]
        public string? VideoUrl { get; set; }

        /// <summary>
        /// anime_title'ı string'e çevirir (sayı veya string olabilir)
        /// </summary>
        public string GetAnimeTitle()
        {
            if (AnimeTitle.ValueKind == System.Text.Json.JsonValueKind.String)
                return AnimeTitle.GetString() ?? string.Empty;
            else if (AnimeTitle.ValueKind == System.Text.Json.JsonValueKind.Number)
                return AnimeTitle.GetInt32().ToString();
            else
                return string.Empty;
        }
    }

    public class MalGenreItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
