using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Services;

public class SessionService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CacheService _cacheService;
    private readonly ApplicationContext _context;

    public SessionService(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        CacheService cacheService,
        ApplicationContext context)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _cacheService = cacheService;
        _context = context;
    }

    public virtual string GenerateToken(User user, List<InstitutionUser> memberships)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("role", user.GlobalRole.ToString())
        };

        // Kurum rolleri claim olarak eklenir
        foreach (var membership in memberships)
        {
            claims.Add(new Claim($"inst_{membership.InstitutionId}", membership.Role.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret not configured")
        ));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public virtual int GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) throw new UnauthorizedAccessException("HttpContext not found");

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }

    public virtual bool IsInGlobalRole(UserRole role)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var roleClaim = user?.FindFirst("role")?.Value;
        return roleClaim == role.ToString();
    }

    public virtual int GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }

    public virtual string? GetInstitutionRole(ClaimsPrincipal user, int institutionId)
    {
        return user.FindFirst($"inst_{institutionId}")?.Value;
    }

    public virtual async Task<UserPermissions> GetUserPermissionsAsync(int userId)
    {
        string cacheKey = $"user_perms_{userId}";

        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var user = await _context.Users
                .Include(u => u.InstitutionMemberships)
                .ThenInclude(im => im.Institution)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null!;

            return new UserPermissions
            {
                UserId = user.Id,
                GlobalRole = user.GlobalRole,
                InstitutionRoles = user.InstitutionMemberships.ToDictionary(
                    im => im.InstitutionId,
                    im => im.Role
                )
            };
        }, TimeSpan.FromHours(1)) ?? null!;
    }

    public virtual async Task InvalidateUserCacheAsync(int userId)
    {
        string cacheKey = $"user_perms_{userId}";
        await _cacheService.RemoveAsync(cacheKey);
    }
}

public class UserPermissions
{
    public int UserId { get; set; }
    public UserRole GlobalRole { get; set; }
    public Dictionary<int, InstitutionRole> InstitutionRoles { get; set; } = new();
}
