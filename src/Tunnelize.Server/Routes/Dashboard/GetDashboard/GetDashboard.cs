namespace Tunnelize.Server.Routes.Dashboard.GetDashboard;

public class GetDashboard : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/dashboard", Handle);
    }

    private static IResult Handle(HttpContext context)
    {
        context.Response.Headers.Append("HX-Redirect", "/dashboard");
        return Results.Empty;
    }
}