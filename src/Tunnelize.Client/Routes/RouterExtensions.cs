using Tunnelize.Client.Routes.ApiKeys.AddApiKey;
using Tunnelize.Client.Routes.ApiKeys.DeleteApiKey;
using Tunnelize.Client.Routes.ApiKeys.GetApiKeyForm;
using Tunnelize.Client.Routes.ApiKeys.ListApiKeys;
using Tunnelize.Client.Routes.Dashboards.GetDashboard;
using Tunnelize.Client.Routes.Sockets.ConnectSocket;
using Tunnelize.Client.Routes.Sockets.DisconnectSocket;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes;

public static class RouterExtensions
{
    public static void MapRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapRoute<ConnectSocket>();
        builder.MapRoute<DisconnectSocket>();
        
        builder.MapRoute<GetApiKeyForm>();
        builder.MapRoute<AddApiKey>();
        builder.MapRoute<ListApiKeys>();
        builder.MapRoute<DeleteApiKey>();
        
        builder.MapRoute<GetDashboard>();
    }
}