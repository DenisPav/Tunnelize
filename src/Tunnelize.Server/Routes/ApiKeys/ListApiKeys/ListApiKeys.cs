using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Server.Components.Dashboards;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Routes.ApiKeys.ListApiKeys;

public class ListApiKeys : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/api-keys", Handle);
    }

    private async Task<RazorComponentResult<DashboardApiKeyList>> Handle(
        DatabaseContext db,
        CancellationToken cancellationToken)
    {
        var mapper = new ApiKeyMapper();
        var entities = await db.Set<ApiKey>()
            .Select(x => mapper.MapToResponse(x))
            .ToListAsync(cancellationToken);

        return new RazorComponentResult<DashboardApiKeyList>(new { ApiKeys = entities });
    }
}