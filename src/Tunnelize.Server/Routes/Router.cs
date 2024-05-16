using Tunnelize.Server.Routes.ApiKeys;
using Tunnelize.Server.Routes.Authentication;

namespace Tunnelize.Server.Routes;

public static class RouterExtensions
{
    public static void MapRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapAuthRoutes();
        builder.MapApiKeyRoutes();
    }
}