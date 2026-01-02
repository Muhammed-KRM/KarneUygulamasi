namespace KeremProject1backend.Models.Responses
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "list_created", "list_updated", "list_shared", "comment_added", "list_liked"
        public string Description { get; set; } = string.Empty;
        public int? RelatedListId { get; set; }
        public string? RelatedListTitle { get; set; }
        public int? RelatedUserId { get; set; }
        public string? RelatedUsername { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

