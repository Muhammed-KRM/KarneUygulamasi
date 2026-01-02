using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class MalCallbackRequest
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty; // code_verifier
    }

    public class GetAnimeListByUsernameRequest
    {
        [Required(ErrorMessage = "Kullanıcı adı boş olamaz.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Kullanıcı adı 1-50 karakter arası olmalı.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Liste durumu: 1=Watching, 2=Completed, 3=On Hold, 4=Dropped, 6=Plan to Watch
        /// </summary>
        public int Status { get; set; } = 2; // Varsayılan: Completed
    }
}
