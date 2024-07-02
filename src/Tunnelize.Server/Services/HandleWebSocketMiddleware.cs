using System.Collections.Concurrent;
using System.Net.WebSockets;
using Injectio.Attributes;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Services;

[RegisterScoped]
public class HandleWebSocketMiddleware : IMiddleware
{
    private const string TunnelizeHeaderKey = "x-tunnelize-key";
    public static readonly IDictionary<string, WebSocket> WebSocketMap = new ConcurrentDictionary<string, WebSocket>();

    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        if (context.WebSockets.IsWebSocketRequest == false)
        {
            await next(context);
            return;
        }
        
        context.Request.Headers.TryGetValue(TunnelizeHeaderKey, out var apiKeyHeader);
        var apiKeyStringValue = apiKeyHeader.FirstOrDefault();
        if (Guid.TryParse(apiKeyStringValue, out var parsedApiKey) == false)
        {
            await next(context);
            return;
        }

        var databaseContext = context.RequestServices.GetRequiredService<DatabaseContext>();
        var apiKey = await databaseContext.Set<ApiKey>()
            .SingleOrDefaultAsync(x => x.Id == parsedApiKey, context.RequestAborted);
        if (apiKey is null)
        {
            await next(context);
            return;
        }

        if (WebSocketMap.TryGetValue(apiKey.SubDomain, out var activeWebSocket))
        {
            activeWebSocket.Abort();
            WebSocketMap.Remove(apiKey.SubDomain);
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        WebSocketMap.Add(apiKey.SubDomain, webSocket);

        while (webSocket.State == WebSocketState.Open)
        {
            await Task.Delay(5000);
        }
    }

    public static async Task ReadFromSocket(WebSocket webSocket)
    {
        var buffer = new byte[65536];
        var segment = new ArraySegment<byte>(buffer);
        WebSocketReceiveResult result;
        var data = new List<byte>();
        do
        {
            result = await webSocket.ReceiveAsync(segment, CancellationToken.None);
            if (result.Count != buffer.Length)
                segment = segment[..result.Count];

            data.AddRange(segment);
        } while (result.EndOfMessage == false);

        segment = new ArraySegment<byte>(data.ToArray());
        await WSocket.DataChannel.Writer.WriteAsync(segment);
    }

    public static async Task WriteToSocket(WebSocket webSocket)
    {
        var totalItems = TcpSocket.DataChannel.Reader.Count;

        for (var i = 0; i < totalItems; i++)
        {
            TcpSocket.DataChannel.Reader.TryRead(out var tcpData);
            var flag = i == totalItems - 1
                ? WebSocketMessageFlags.EndOfMessage
                : WebSocketMessageFlags.None;
            
            await webSocket.SendAsync(
                tcpData,
                WebSocketMessageType.Binary,
                flag,
                CancellationToken.None);
        }
    }
}