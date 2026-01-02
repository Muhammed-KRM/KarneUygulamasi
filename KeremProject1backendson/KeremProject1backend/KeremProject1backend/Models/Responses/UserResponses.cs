namespace KeremProject1backend.Models.Responses
{
    public class GetUserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string UserImageLink { get; set; } = string.Empty;
        public string? MalUsername { get; set; }
        public DateTime ModTime { get; set; }
    }

    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UserImageLink { get; set; } = string.Empty;
        public string? MalUsername { get; set; }
        public int TotalLists { get; set; }
        public int TotalFollowers { get; set; }
        public DateTime ModTime { get; set; }
    }

    public class UserListResponse
    {
        public List<UserSummaryDto> Users { get; set; } = new List<UserSummaryDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages { get; set; }
    }
}
