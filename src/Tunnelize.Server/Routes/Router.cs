using Tunnelize.Server.Routes.ApiKeys.CreateApiKeys;
using Tunnelize.Server.Routes.ApiKeys.DeleteApiKey;
using Tunnelize.Server.Routes.ApiKeys.GetCreateApiKey;
using Tunnelize.Server.Routes.ApiKeys.ListApiKeys;
using Tunnelize.Server.Routes.Authentication;

namespace Tunnelize.Server.Routes;

public static class RouterExtensions
{
    public static void MapRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapAuthRoutes();
        
        builder.MapRoute<CreateApiKeys>();
        builder.MapRoute<GetCreateApiKeys>();
        builder.MapRoute<ListApiKeys>();
        builder.MapRoute<DeleteApiKey>();
    }
}