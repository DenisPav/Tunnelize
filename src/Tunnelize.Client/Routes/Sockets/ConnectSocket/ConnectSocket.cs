using Microsoft.AspNetCore.Http.HttpResults;
using Tunnelize.Client.Components.Dashboards;
using Tunnelize.Client.Sockets;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.Sockets.ConnectSocket;

public class ConnectSocket : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/sockets/connect", Handle);
    }

    private IResult Handle()
    {
        WebSocketHandler.CreateWebSocket();

        return new RazorComponentResult<Dashboard>(new { IsConnected = true });
    }
}