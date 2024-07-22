using Microsoft.AspNetCore.Http.HttpResults;
using Tunnelize.Client.Components.Dashboards;
using Tunnelize.Client.Sockets;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.Sockets.DisconnectSocket;

public class DisconnectSocket : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/sockets/disconnect", Handle);
    }

    private static IResult Handle()
    {
        WebSocketHandler.CloseSocket();
        TcpSocketHandler.Port = null;

        return new RazorComponentResult<Dashboard>(new { IsConnected = false });
    }
}