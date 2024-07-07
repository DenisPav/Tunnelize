using Microsoft.EntityFrameworkCore;
using Tunnelize.Client.Persistence;
using Tunnelize.Client.Persistence.Entities;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.ApiKeys.DeleteApiKey;

public class DeleteApiKey : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/api/api-keys/{id:Guid}", Handle);
    }

    private static async Task<IResult> Handle(
        Guid id,
        HttpContext context,
        DatabaseContext db,
        CancellationToken cancellationToken)
    {
        await db.Set<ApiKey>()
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        
        context.Response.Headers.Append("HX-Redirect", "/");
        return Results.Empty;
    }
}