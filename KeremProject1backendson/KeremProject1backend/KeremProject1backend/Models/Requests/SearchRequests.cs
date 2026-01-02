using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class SearchAnimeRequest
    {
        public string? Query { get; set; }
        public string? Genre { get; set; }
        public int? Year { get; set; }
        public double? MinScore { get; set; }
        public double? MaxScore { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 25;
    }
}

