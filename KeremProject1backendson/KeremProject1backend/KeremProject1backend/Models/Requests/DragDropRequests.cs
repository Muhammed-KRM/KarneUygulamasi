using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class MoveItemRequest
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        public int TargetTierId { get; set; }

        [Required]
        public int NewRankInTier { get; set; }
    }

    public class ReorderItemsRequest
    {
        [Required]
        public int TierId { get; set; }

        [Required]
        public List<ItemOrderDto> Items { get; set; } = new List<ItemOrderDto>();
    }

    public class ItemOrderDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        public int RankInTier { get; set; }
    }
}

