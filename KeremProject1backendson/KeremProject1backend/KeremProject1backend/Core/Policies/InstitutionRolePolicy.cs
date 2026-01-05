using KeremProject1backend.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace KeremProject1backend.Core.Policies;

public class RequireInstitutionRoleAttribute : AuthorizeAttribute
{
    public RequireInstitutionRoleAttribute(params InstitutionRole[] roles)
    {
        var rolesString = string.Join(",", roles.Select(r => r.ToString()));
        Policy = $"InstitutionRole:{rolesString}";
    }
}

