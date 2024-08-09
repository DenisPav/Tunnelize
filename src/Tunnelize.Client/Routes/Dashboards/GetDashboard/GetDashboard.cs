using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.Dashboards.GetDashboard;

public class GetDashboard : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/dashboards", Handle);
    }

    private static IResult Handle(HttpContext context) 
        => context.HtmxRedirect( "/");
}