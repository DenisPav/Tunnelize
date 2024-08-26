using Microsoft.EntityFrameworkCore;
using Tunnelize.Client.Persistence;
using Tunnelize.Client.Persistence.Entities;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.ApiKeys.ListApiKeys;

public class UpdateApiKey : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/api/api-keys/{id:Guid}/toggle", Handle);
    }
    
    private static async Task<IResult> Handle(
        Guid id,
        HttpContext context,
        DatabaseContext db,
        CancellationToken cancellationToken)
    {
        await db.Set<ApiKey>()
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                apiKey => apiKey.SetProperty(prop => prop.IsActive, prop => !prop.IsActive),
                cancellationToken);

        context.HtmxRedirect("/");
        return TypedResults.Empty;
    }
}