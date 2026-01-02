using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class CopyListRequest
    {
        [Required]
        public int SourceListId { get; set; }

        [Required]
        [StringLength(100)]
        public string NewTitle { get; set; } = string.Empty;
    }
}

