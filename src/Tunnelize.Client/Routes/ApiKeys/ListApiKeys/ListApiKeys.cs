using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Client.Components.ApiKeys;
using Tunnelize.Client.Components.Dashboards;
using Tunnelize.Client.Persistence;
using Tunnelize.Client.Persistence.Entities;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.ApiKeys.ListApiKeys;

public class ListApiKeys : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/api-keys", Handle);
    }
    
    private static async Task<IResult> Handle(
        HttpContext context,
        DatabaseContext db,
        CancellationToken cancellationToken)
    {
        var mapper = new ApiKeyMapper();
        var apiKeys = await db.Set<ApiKey>()
            .Select(x => mapper.MapToResponse(x))
            .ToListAsync(cancellationToken);
        
        return new RazorComponentResult<DashboardApiKeyList>(new { ApiKeys = apiKeys });
    }
}