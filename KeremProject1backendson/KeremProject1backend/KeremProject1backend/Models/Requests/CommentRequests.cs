using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class AddCommentRequest
    {
        [Required]
        public int ListId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateCommentRequest
    {
        [Required]
        public int CommentId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }
}

