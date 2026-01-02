using System.Text.Json.Serialization;

namespace KeremProject1backend.Models.Responses
{
    public class SearchAnimeResultDto
    {
        public int MalId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double? Score { get; set; }
        public int? Year { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public string Synopsis { get; set; } = string.Empty;
    }

    public class SearchAnimeResponse
    {
        public List<SearchAnimeResultDto> Results { get; set; } = new List<SearchAnimeResultDto>();
        public int TotalResults { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }

    // MAL API v2 response modelleri
    public class MalApiV2AnimeListResponse
    {
        [JsonPropertyName("data")]
        public List<MalApiV2AnimeNode> Data { get; set; } = new List<MalApiV2AnimeNode>();

        [JsonPropertyName("paging")]
        public MalApiV2Paging? Paging { get; set; }
    }

    public class MalApiV2AnimeNode
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("main_picture")]
        public MalApiV2Picture? MainPicture { get; set; }

        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string? EndDate { get; set; }

        [JsonPropertyName("synopsis")]
        public string? Synopsis { get; set; }

        [JsonPropertyName("mean")]
        public double? Mean { get; set; }

        [JsonPropertyName("genres")]
        public List<MalApiV2Genre> Genres { get; set; } = new List<MalApiV2Genre>();
    }

    public class MalApiV2Picture
    {
        [JsonPropertyName("medium")]
        public string? Medium { get; set; }

        [JsonPropertyName("large")]
        public string? Large { get; set; }
    }

    public class MalApiV2Genre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class MalApiV2Paging
    {
        [JsonPropertyName("previous")]
        public string? Previous { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }
    }
}

