using Tunnelize.Client.Routes.Sockets.ConnectSocket;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes;

public static class RouterExtensions
{
    public static void MapRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapRoute<ConnectSocket>();
    }
}