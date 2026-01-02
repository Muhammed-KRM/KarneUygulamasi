using System.Text.Json.Serialization;

namespace KeremProject1backend.Models.Responses
{
    public class JikanAnimeResponse
    {
        [JsonPropertyName("data")]
        public JikanData Data { get; set; } = new JikanData();
    }

    public class JikanData
    {
        [JsonPropertyName("mal_id")]
        public int MalId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("score")]
        public double? Score { get; set; }

        [JsonPropertyName("images")]
        public JikanImages Images { get; set; } = new JikanImages();

        [JsonPropertyName("genres")]
        public List<JikanDetail> Genres { get; set; } = new List<JikanDetail>();

        [JsonPropertyName("themes")]
        public List<JikanDetail> Themes { get; set; } = new List<JikanDetail>();
    }

    public class JikanDetail
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class JikanImages
    {
        [JsonPropertyName("jpg")]
        public JikanImageUrls Jpg { get; set; } = new JikanImageUrls();
    }

    public class JikanImageUrls
    {
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
