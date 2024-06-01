using System.Security.Claims;

namespace Tunnelize.Server.Routes;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var rawUserIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (rawUserIdClaim is null)
            throw new Exception($"User claim not found: [{ClaimTypes.NameIdentifier}]");
        
        return Guid.Parse(rawUserIdClaim.Value);
    }
}