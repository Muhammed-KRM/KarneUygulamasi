using KeremProject1backend.Models.DBs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KeremProject1backend.Services
{
    public static class TokenServices
    {
        private static string _secretKey = string.Empty;
        private static readonly int TokenExpirationMinutes = 480; // 8 Saat

        public static void Initialize(IConfiguration config)
        {
            _secretKey = config["AppSettings:TokenSecret"]
                ?? throw new InvalidOperationException("AppSettings:TokenSecret bulunamadı.");
        }

        public static string GenerateToken(AppUser user)
        {
            if (string.IsNullOrEmpty(_secretKey))
                throw new InvalidOperationException("TokenService başlatılmadı (Initialize çağrılmadı).");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.UserRole.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(TokenExpirationMinutes),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(_secretKey))
                throw new InvalidOperationException("TokenService başlatılmadı.");

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                return null; // Token geçersiz
            }
        }
    }
}
