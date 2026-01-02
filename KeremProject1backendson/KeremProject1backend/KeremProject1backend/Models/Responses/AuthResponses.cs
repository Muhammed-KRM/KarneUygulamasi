namespace KeremProject1backend.Models.Responses
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string? MalUsername { get; set; } // MAL hesabı bağlı mı?
    }
}
