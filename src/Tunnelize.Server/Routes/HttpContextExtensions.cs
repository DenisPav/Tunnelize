using System.Security.Claims;

namespace Tunnelize.Server.Routes;

public static class HttpContextExtensions
{
    public static Guid GetUserId(this HttpContext context)
    {
        var rawUserIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (rawUserIdClaim is null)
            throw new Exception($"User claim not found: [{ClaimTypes.NameIdentifier}]");
        
        return Guid.Parse(rawUserIdClaim.Value);
    }
}