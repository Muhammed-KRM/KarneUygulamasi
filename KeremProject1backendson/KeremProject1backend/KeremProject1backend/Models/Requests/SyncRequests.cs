using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class SyncMalListRequest
    {
        [Required]
        public int ListId { get; set; }

        public bool ReplaceExisting { get; set; } = false; // Mevcut item'ları değiştir mi?
    }
}

