using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Channels;
using Microsoft.AspNetCore.WebSockets;

CreateTcpListener();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebSockets(_ => { });
builder.Services.AddScoped<HandleWebSocketMiddleware>();
var app = builder.Build();

app.UseWebSockets();
app.UseMiddleware<HandleWebSocketMiddleware>();
app.MapGet("/", () => "Hello World!");
app.Run();


async void CreateTcpListener()
{
    var listener = new TcpListener(IPAddress.Loopback, 8080);
    listener.Start();

    while (true)
    {
        try
        {
            var socket = await listener.AcceptSocketAsync();

            await TcpSocket.ReadFromTcpSocket(socket);
            await HandleWebSocketMiddleware.WriteToSocket(HandleWebSocketMiddleware.CurrentWebSocket);
            await HandleWebSocketMiddleware.ReadFromSocket(HandleWebSocketMiddleware.CurrentWebSocket);

            await TcpSocket.WriteToTcpSocket(socket);
            socket.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}

public static class TcpSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();

    public static async Task ReadFromTcpSocket(Socket socket)
    {
        var bytes = new byte[socket.ReceiveBufferSize];
        var dataBuffer = new ArraySegment<byte>(bytes);
        int numberOfBytes;

        while ((numberOfBytes = await socket.ReceiveAsync(dataBuffer)) != 0)
        {
            if (numberOfBytes != bytes.Length)
                dataBuffer = dataBuffer[..numberOfBytes];

            await DataChannel.Writer.WriteAsync(dataBuffer);

            return;
        }
    }

    public static async Task WriteToTcpSocket(Socket socket)
    {
        var combinedData = new List<byte>();
        while (true)
        {
            var readSuccess = WSocket.DataChannel.Reader.TryRead(out var tcpData);
            if (readSuccess == false)
                break;

            combinedData.AddRange(tcpData);
        }

        await socket.SendAsync(new ArraySegment<byte>(combinedData.ToArray()));
    }
}

public static class WSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();
}

public class HandleWebSocketMiddleware : IMiddleware
{
    public static WebSocket CurrentWebSocket;

    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        if (context.WebSockets.IsWebSocketRequest == false)
        {
            await next(context);
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        CurrentWebSocket = webSocket;
        while (webSocket.State == WebSocketState.Open)
        {
            await Task.Delay(Timeout.Infinite);
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