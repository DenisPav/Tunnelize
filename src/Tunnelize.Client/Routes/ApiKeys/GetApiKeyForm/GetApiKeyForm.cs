using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.ApiKeys.GetApiKeyForm;

public class GetApiKeyForm : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/api-keys/form", Handle);
    }

    private static IResult Handle(HttpContext context)
    {
        context.Response.Headers.Append("HX-Redirect", "/add");
        return Results.Empty;
    }
}