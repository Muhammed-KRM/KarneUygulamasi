using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class FollowUserRequest
    {
        [Required]
        public int UserId { get; set; }
    }

    public class CreateTemplateRequest
    {
        [Required]
        public int ListId { get; set; }

        [Required]
        [StringLength(100)]
        public string TemplateName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}

