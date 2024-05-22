namespace Tunnelize.Server.Routes.ApiKeys.DeleteApiKey;

public class DeleteApiKey : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/api-keys/delete/{id}", () => TypedResults.Empty);
    }
}