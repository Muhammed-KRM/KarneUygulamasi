namespace KeremProject1backend.Models.Responses
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ModTime { get; set; }
        public bool IsOwnComment { get; set; }
    }
}

