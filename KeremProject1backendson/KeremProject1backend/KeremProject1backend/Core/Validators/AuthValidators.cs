using FluentValidation;
using KeremProject1backend.Models.Requests;

namespace KeremProject1backend.Core.Validators
{
    // Örnek: Login Request için Validator
    // Models/Requests klasöründe Auth requestleri henüz oluşturulmadıysa bu dosya hata verebilir
    // Ancak yapıyı kurmak adına oluşturuyorum.
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email adresi boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
        }
    }

    // LoginRequest sınıfı henüz yoksa dummy olarak burada tanımlayalım ki derleme hatası almasın şimdilik
    // Normalde Models/Requests altında olmalı.
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
