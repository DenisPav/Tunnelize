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
    public static IDictionary<string, WebSocket> WebSocketMap = new ConcurrentDictionary<string, WebSocket>();

    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        if (context.WebSockets.IsWebSocketRequest == false)
        {
            await next(context);
            return;
        }

        context.Request.Headers.TryGetValue("x-tunnelize-key", out var apiKeyHeader);
        var apiKeyStringValue = apiKeyHeader.FirstOrDefault();
        if (Guid.TryParse(apiKeyStringValue, out var parsedApiKey) == false)
        {
            await next(context);
            return;
        }

        var databaseContext = context.RequestServices.GetRequiredService<DatabaseContext>();
        var apiKey = await databaseContext.Set<ApiKey>().SingleOrDefaultAsync(x => x.Id == parsedApiKey, context.RequestAborted);
        if (apiKey is null)
        {
            await next(context);
            return;
        }

        if (WebSocketMap.ContainsKey(apiKey.SubDomain))
        {
            await next(context);
            return;
        }
        
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        WebSocketMap.Add(apiKey.SubDomain, webSocket);
        
        while (webSocket.State == WebSocketState.Open)
        {
            await Task.Delay(20000);
        }

        WebSocketMap.Remove(apiKey.SubDomain);
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
        var data = new List<byte>();
        while (true)
        {
            var readSuccess = TcpSocket.DataChannel.Reader.TryRead(out var readData);
            if (readSuccess == false)
                break;

            data.AddRange(readData);
        }

        var tcpData = new ArraySegment<byte>(data.ToArray());
        await webSocket.SendAsync(
            tcpData,
            WebSocketMessageType.Binary,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None);
    }
}