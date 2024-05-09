using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;

async void CreateWebSocket()
{
    try
    {
        var webSocket = new ClientWebSocket();
        var serverLocation = new Uri("ws://127.0.0.1:5000");
        await webSocket.ConnectAsync(serverLocation, CancellationToken.None);

        while (webSocket.State == WebSocketState.Open)
        {
            await Task.WhenAll(ReadFromSocket(webSocket), WriteToSocket(webSocket));
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
    Console.WriteLine("Reading from socket");

    var buffer = new byte[65536];
    var segment = new ArraySegment<byte>(buffer);

    await webSocket.ReceiveAsync(segment, CancellationToken.None);

    await WSocket.DataChannel.Writer.WriteAsync(segment);
}

async Task WriteToSocket(WebSocket webSocket)
{
    Console.WriteLine("Writing to socket");
    
    while (await TcpSocket.DataChannel.Reader.WaitToReadAsync())
    {
        var tcpData = await TcpSocket.DataChannel.Reader.ReadAsync();
        await webSocket.SendAsync(tcpData, WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None);
    }
}

async void CreateTcpSocket()
{
    try
    {
        var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        await tcpSocket.ConnectAsync("127.0.0.1", 3000);
        
        while (tcpSocket.Connected)
        {
            await Task.WhenAll(TcpSocket.ReadFromTcpSocket(tcpSocket), TcpSocket.WriteToTcpSocket(tcpSocket));
        }

        Console.WriteLine("closing TCP client socket");
        tcpSocket.Close();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

CreateWebSocket();
CreateTcpSocket();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();


public static class WSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();
}

public static class TcpSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();

    public static async Task ReadFromTcpSocket(Socket socket)
    {
        Console.WriteLine("Reading from TCP socket");

        var bytes = new byte[socket.ReceiveBufferSize];
        var dataBuffer = new ArraySegment<byte>(bytes);
        _ = await socket.ReceiveAsync(dataBuffer);

        await DataChannel.Writer.WriteAsync(dataBuffer);
    }

    public static async Task WriteToTcpSocket(Socket socket)
    {
        Console.WriteLine("Writing to TCP socket");
        
        while (await WSocket.DataChannel.Reader.WaitToReadAsync())
        {
            var tcpData = await WSocket.DataChannel.Reader.ReadAsync();
            await socket.SendAsync(tcpData);
        }
    }
}