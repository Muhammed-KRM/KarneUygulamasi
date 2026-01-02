using System.ComponentModel.DataAnnotations;
using KeremProject1backend.Models.DBs;

namespace KeremProject1backend.Models.Requests
{
    public class GetAllUsersRequest
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
        public string? SearchQuery { get; set; }
        public bool? IsActive { get; set; } // State kontrolü için
    }

    public class SearchUsersRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Query { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
    }

    public class UpdateUserRequest
    {
        public int? TargetUserId { get; set; } // Admin için: Güncellenecek kullanıcı ID'si (null ise kendi hesabı)
        public string? Username { get; set; }
        public string? MalUsername { get; set; }
        public UserRole? Role { get; set; } // Admin için: Kullanıcı rolü güncelleme
        public bool? State { get; set; } // Admin için: Kullanıcı durumu (aktif/pasif)
    }

    public class AdminUpdateUserRequest
    {
        [Required]
        public int TargetUserId { get; set; }
        public string? Username { get; set; }
        public string? MalUsername { get; set; }
        public UserRole? Role { get; set; }
        public bool? State { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        [StringLength(50)]
        public string? Username { get; set; }

        [StringLength(50)]
        public string? MalUsername { get; set; }
    }

    public class DeleteUserRequest
    {
        [Required]
        public int UserId { get; set; }
        public bool HardDelete { get; set; } = false; // true ise kalıcı sil, false ise soft delete (State = false)
        public string? Password { get; set; } // Kendi hesabını silerken şifre doğrulama için
    }
}
