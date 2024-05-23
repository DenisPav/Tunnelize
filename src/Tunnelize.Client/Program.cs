using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Channels;

async void CreateWebSocket()
{
    try
    {
        var webSocket = new ClientWebSocket();
        webSocket.Options.SetRequestHeader("x-tunnelize-key", "73441f56-7751-440e-a26c-0543a03ca9ad");
        var serverLocation = new Uri("ws://127.0.0.1:5000");
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
    var buffer = new byte[65536];
    var segment = new ArraySegment<byte>(buffer);

    await webSocket.ReceiveAsync(segment, CancellationToken.None);
    await WSocket.DataChannel.Writer.WriteAsync(segment);

    await CreateTcpSocket();
}

async Task WriteToSocket(WebSocket webSocket)
{
    await TcpSocket.DataChannel.Reader.WaitToReadAsync();

    var tcpData = await TcpSocket.DataChannel.Reader.ReadAsync();
    var isLastItem = TcpSocket.DataChannel.Reader.Count == 0;
    var messageFlag = isLastItem
        ? WebSocketMessageFlags.EndOfMessage
        : WebSocketMessageFlags.None;

    await webSocket.SendAsync(tcpData, WebSocketMessageType.Binary, messageFlag, CancellationToken.None);
}

async Task CreateTcpSocket()
{
    try
    {
        var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        
        await tcpSocket.ConnectAsync("127.0.0.1", 3000);
        await TcpSocket.WriteToTcpSocket(tcpSocket);
        await TcpSocket.ReadFromTcpSocket(tcpSocket);

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
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();
}

public static class TcpSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();

    public static async Task ReadFromTcpSocket(Socket socket)
    {
        var bytes = new byte[socket.ReceiveBufferSize];
        var dataBuffer = new ArraySegment<byte>(bytes);
        int numberOfBytes;
        var finalData = new List<byte>();

        while ((numberOfBytes = await socket.ReceiveAsync(dataBuffer)) != 0)
        {
            if (numberOfBytes != bytes.Length)
                dataBuffer = dataBuffer[..numberOfBytes];
            finalData.AddRange(dataBuffer);
        }

        if (finalData.Count == 0)
            return;

        dataBuffer = new ArraySegment<byte>(finalData.ToArray());
        await DataChannel.Writer.WriteAsync(dataBuffer);
    }

    public static async Task WriteToTcpSocket(Socket socket)
    {
        await WSocket.DataChannel.Reader.WaitToReadAsync();
        var tcpData = await WSocket.DataChannel.Reader.ReadAsync();
        await socket.SendAsync(tcpData);
    }
}