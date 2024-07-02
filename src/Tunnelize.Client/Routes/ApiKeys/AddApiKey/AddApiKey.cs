using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.ApiKeys.AddApiKey;

public class AddApiKey : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/api-keys", Handle);
    }

    private IResult Handle(HttpContext context)
    {
        return Results.Empty; 
    }
}