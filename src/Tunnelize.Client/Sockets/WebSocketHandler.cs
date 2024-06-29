using System.Net.WebSockets;

namespace Tunnelize.Client.Sockets;

public static class WebSocketHandler
{
    public static bool IsConnected;
    
    public static async void CreateWebSocket()
    {
        try
        {
            var webSocket = new ClientWebSocket();
            webSocket.Options.SetRequestHeader("x-tunnelize-key", "1de76071-b172-4f05-9a4a-a1a0d2daa21b");
            webSocket.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            var serverLocation = new Uri("ws://localhost:5000");
            await webSocket.ConnectAsync(serverLocation, CancellationToken.None);

            IsConnected = true;

            while (webSocket.State == WebSocketState.Open)
            {
                await ReadFromSocket(webSocket);
                await WriteToSocket(webSocket);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            IsConnected = false;
        }
    }
    
    static async Task ReadFromSocket(WebSocket webSocket)
    {
        WebSocketReceiveResult result;
        do
        {
            var buffer = new byte[65536];
            var segment = new ArraySegment<byte>(buffer);
            result = await webSocket.ReceiveAsync(segment, CancellationToken.None);
            if (result.Count != buffer.Length)
                segment = segment[..result.Count];
    
            await WSocket.DataChannel.Writer.WriteAsync(segment);
        } while (result.EndOfMessage == false);
        
        await TcpSocketHandler.CreateTcpSocket();
    }
    
    static async Task WriteToSocket(WebSocket webSocket)
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