using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        [FromForm] ConnectSocketRequest request,
        DatabaseContext db,
        IValidator<ConnectSocketRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
        {
            return new RazorComponentResult<Dashboard>(new { HasErrors = true });
        }
        
        var activeApiKey = await db.Set<ApiKey>()
            .Where(x => x.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        TcpSocketHandler.Port = request.Port;
        var isConnected = await WebSocketHandler.CreateWebSocket(activeApiKey!.Value);
        WebSocketHandler.HandleWebSocket();
        return new RazorComponentResult<Dashboard>(new { IsConnected = isConnected });
    }
}