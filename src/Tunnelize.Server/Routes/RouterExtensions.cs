using Tunnelize.Server.Routes.ApiKeys.CreateApiKeys;
using Tunnelize.Server.Routes.ApiKeys.DeleteApiKey;
using Tunnelize.Server.Routes.ApiKeys.GetCreateApiKey;
using Tunnelize.Server.Routes.ApiKeys.ListApiKeys;
using Tunnelize.Server.Routes.Authentication.Login;
using Tunnelize.Server.Routes.Authentication.LoginCode;
using Tunnelize.Server.Routes.Authentication.Logout;
using Tunnelize.Server.Routes.Dashboard.GetDashboard;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Server.Routes;

public static class RouterExtensions
{
    public static void MapRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapRoute<Login>();
        builder.MapRoute<LoginCode>();
        builder.MapRoute<Logout>();
        
        builder.MapRoute<CreateApiKeys>();
        builder.MapRoute<GetCreateApiKeys>();
        builder.MapRoute<ListApiKeys>();
        builder.MapRoute<DeleteApiKey>();
        
        builder.MapRoute<GetDashboard>();
    }
}