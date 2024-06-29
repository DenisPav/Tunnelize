using Microsoft.AspNetCore.Http.HttpResults;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Server.Routes.ApiKeys.GetCreateApiKey;

public class GetCreateApiKeys : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/api-keys/create", Handle);
    }

    private static EmptyHttpResult Handle(HttpContext context)
    {
        context.Response.Headers.Append("HX-Redirect", "/create");
        return TypedResults.Empty;
    }
}