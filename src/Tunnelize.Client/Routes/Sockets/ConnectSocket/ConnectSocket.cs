using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Client.Components.Dashboards;
using Tunnelize.Client.Persistence;
using Tunnelize.Client.Persistence.Entities;
using Tunnelize.Client.Sockets;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.Sockets.ConnectSocket;

public class ConnectSocket : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/sockets/connect", Handle);
    }

    private async Task<IResult> Handle(
        DatabaseContext db,
        CancellationToken cancellationToken)
    {
        var activeApiKey = await db.Set<ApiKey>()
            .Where(x => x.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
        
        WebSocketHandler.CreateWebSocket(activeApiKey!.Value);
        return new RazorComponentResult<Dashboard>(new { IsConnected = true });
    }
}