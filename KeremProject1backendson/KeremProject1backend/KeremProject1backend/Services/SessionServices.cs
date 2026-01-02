using KeremProject1backend.Models.DBs;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Authentication;
using System.IdentityModel.Tokens.Jwt;

namespace KeremProject1backend.Services
{
    public static class SessionService
    {
        public static ClaimsPrincipal? TestToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            // "Bearer " önekini kaldır (varsa)
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring(7);
            }

            return TokenServices.ValidateToken(token);
        }

        public static int GetUserId(ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new SecurityTokenException("Token içinde geçerli bir kullanıcı ID'si bulunamadı.");
            }
            return userId;
        }

        public static bool isAuthorized(ClaimsPrincipal session, UserRole requiredRole)
        {
            var roleClaim = session.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
                return false;

            if (Enum.TryParse(roleClaim.Value, out UserRole userRole))
            {
                // AdminAdmin her şeyi yapabilir
                if (userRole == UserRole.AdminAdmin)
                    return true;

                // Admin, User rolü gerektiren her şeyi yapabilir (ama Admin atayamaz)
                if (userRole == UserRole.Admin)
                {
                    // Admin, AdminAdmin veya Admin rolü atayamaz
                    if (requiredRole == UserRole.Admin || requiredRole == UserRole.AdminAdmin)
                        return false;
                    return true;
                }

                return userRole == requiredRole;
            }

            return false;
        }
    }
}
