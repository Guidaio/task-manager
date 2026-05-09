using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace TaskManager.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? TryGetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
