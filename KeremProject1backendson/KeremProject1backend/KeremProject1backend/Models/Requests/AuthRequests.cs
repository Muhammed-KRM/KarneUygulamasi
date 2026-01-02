using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Kullanıcı adı boş olamaz.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arası olmalı.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola boş olamaz.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola en az 6 karakter olmalı.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Kullanıcı adı boş olamaz.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola boş olamaz.")]
        public string Password { get; set; } = string.Empty;
    }

    public class CreateAdminDto
    {
        [Required(ErrorMessage = "Kullanıcı adı boş olamaz.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arası olmalı.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola boş olamaz.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola en az 6 karakter olmalı.")]
        public string Password { get; set; } = string.Empty;
    }

    public class CreateAdminAdminDto
    {
        [Required(ErrorMessage = "Kullanıcı adı boş olamaz.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arası olmalı.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola boş olamaz.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola en az 6 karakter olmalı.")]
        public string Password { get; set; } = string.Empty;
    }
}
