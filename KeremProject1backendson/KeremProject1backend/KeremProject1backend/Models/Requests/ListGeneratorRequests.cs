using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class GenerateByGenreRequest
    {
        [Required]
        public string GenreTag { get; set; } = string.Empty; // Örn: "Comedy"
    }
}
