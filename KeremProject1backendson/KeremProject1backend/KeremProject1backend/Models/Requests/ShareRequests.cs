using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class SetListVisibilityRequest
    {
        [Required]
        public int ListId { get; set; }

        [Required]
        public bool IsPublic { get; set; }
    }

    public class GenerateShareLinkRequest
    {
        [Required]
        public int ListId { get; set; }
    }
}

