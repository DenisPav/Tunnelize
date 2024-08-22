using Tunnelize.Shared.Routes;

namespace Tunnelize.Server.Routes.Dashboard.GetDashboard;

public class GetDashboard : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/dashboard", Handle);
    }

    private static IResult Handle(HttpContext context)
        => context.HtmxRedirect("/dashboard");
}