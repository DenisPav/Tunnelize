using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Channels;

async void CreateWebSocket()
{
    try
    {
        var webSocket = new ClientWebSocket();
        webSocket.Options.SetRequestHeader("x-tunnelize-key", "1de76071-b172-4f05-9a4a-a1a0d2daa21b");
        webSocket.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        var serverLocation = new Uri("ws://localhost:5000");
        await webSocket.ConnectAsync(serverLocation, CancellationToken.None);

        while (webSocket.State == WebSocketState.Open)
        {
            await ReadFromSocket(webSocket);
            await WriteToSocket(webSocket);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

async Task ReadFromSocket(WebSocket webSocket)
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
    
    await CreateTcpSocket();
}

async Task WriteToSocket(WebSocket webSocket)
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

async Task CreateTcpSocket()
{
    ServicePointManager.SetTcpKeepAlive(true, 1000, 1000);

    try
    {
        var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        await tcpSocket.ConnectAsync("127.0.0.1", 3000);
        await TcpSocket.WriteToTcpSocket(tcpSocket);
        await TcpSocket.ReadFromTcpSocket(tcpSocket);

        tcpSocket.Shutdown(SocketShutdown.Both);
        tcpSocket.Close();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

CreateWebSocket();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();


public static class WSocket
{
    public static readonly Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();
}

public static class TcpSocket
{
    public static readonly Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();

    public static async Task ReadFromTcpSocket(Socket socket)
    {
        var bytes = new byte[socket.ReceiveBufferSize];
        var dataBuffer = new ArraySegment<byte>(bytes);
        var numberOfBytes = 0;
        
        try
        {
            while ((numberOfBytes = socket.Receive(dataBuffer)) != 0)
            {
                if (numberOfBytes != bytes.Length)
                {
                    dataBuffer = dataBuffer[..numberOfBytes];
                }

                await DataChannel.Writer.WriteAsync(dataBuffer);
                
                //document this, try with async stuff and also add this to configuration
                if (!socket.Poll(10000, SelectMode.SelectRead))
                    break;

                bytes = new byte[socket.ReceiveBufferSize];
                dataBuffer = new ArraySegment<byte>(bytes);
            }
        }
        catch
        {
            // ignored
        }
    }

    public static async Task WriteToTcpSocket(Socket socket)
    {
        await WSocket.DataChannel.Reader.WaitToReadAsync();
        var totalItems = WSocket.DataChannel.Reader.Count;

        for (var i = 0; i < totalItems; i++)
        {
            WSocket.DataChannel.Reader.TryRead(out var tcpData);
            await socket.SendAsync(tcpData);
        }
    }
}