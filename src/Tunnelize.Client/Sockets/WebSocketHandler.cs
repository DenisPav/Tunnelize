using System.Net.WebSockets;
using System.Threading.Channels;

namespace Tunnelize.Client.Sockets;

public static class WebSocketHandler
{
    public static readonly Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();
    public static bool IsConnected;
    
    private static ClientWebSocket? WebSocket;

    public static async Task<bool> CreateWebSocket(string apiKey)
    {
        try
        {
            WebSocket = new ClientWebSocket();
            WebSocket.Options.SetRequestHeader("x-tunnelize-key", apiKey);
            WebSocket.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            var serverLocation = new Uri("wss://tunnelize.xyz");
            
#if DEBUG
            serverLocation = new Uri("ws://localhost:5000");
#endif
            await WebSocket.ConnectAsync(serverLocation, CancellationToken.None);

            IsConnected = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            IsConnected = false;
        }
        
        return IsConnected;
    }

    public static async void HandleWebSocket()
    {
        try
        {
            while (WebSocket!.State == WebSocketState.Open)
            {
                await ReadFromSocket(WebSocket);
                await WriteToSocket(WebSocket);
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

    public static void CloseSocket()
    {
        if (WebSocket is not { } socket)
        {
            return;
        }

        socket.Abort();
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

            await DataChannel.Writer.WriteAsync(segment);
        } while (result.EndOfMessage == false);

        await TcpSocketHandler.CreateTcpSocket();
    }

    static async Task WriteToSocket(WebSocket webSocket)
    {
        var totalItems = TcpSocketHandler.DataChannel.Reader.Count;

        for (var i = 0; i < totalItems; i++)
        {
            TcpSocketHandler.DataChannel.Reader.TryRead(out var tcpData);
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