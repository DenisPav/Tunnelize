using Microsoft.EntityFrameworkCore;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Routes.ApiKeys.DeleteApiKey;

public class DeleteApiKey : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/api/api-keys/delete/{id:guid}", async (
            Guid id, 
            DatabaseContext db,
            CancellationToken cancellationToken) =>
        {
            await db.Set<ApiKey>()
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
            
            return TypedResults.Empty;
        });
    }
}