namespace KeremProject1backend.Models.DBs
{
    public enum UserRole
    {
        User = 0,
        Admin = 1,
        AdminAdmin = 2
    }

    public class AppUser
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public UserRole UserRole { get; set; } = UserRole.User;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public bool State { get; set; }
        public string UserImageLink { get; set; } = string.Empty;
        public DateTime ModTime { get; set; }
        public int ModUser { get; set; }

        // Navigation properties
        public ICollection<UserFollow> Following { get; set; } = new List<UserFollow>(); // Takip ettiği kullanıcılar
        public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>(); // Takipçileri
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    // Takipçi sistemi
    public class UserFollow
    {
        public int Id { get; set; }
        public int FollowerId { get; set; } // Takip eden
        public int FollowingId { get; set; } // Takip edilen
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public AppUser Follower { get; set; } = null!;
        public AppUser Following { get; set; } = null!;
    }

    // Bildirimler
    public class Notification
    {
        public int Id { get; set; }
        public int AppUserId { get; set; }
        public string Type { get; set; } = string.Empty; // "like", "comment", "follow", "mention"
        public string Message { get; set; } = string.Empty;
        public int? RelatedUserId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public AppUser AppUser { get; set; } = null!;
    }
}
