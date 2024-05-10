using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Unicode;
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
    try
    {
        listener.Start();
        Console.WriteLine("Started TCP listener");

        var socket = await listener.AcceptSocketAsync();

        while (socket.Connected)
        {
            await Task.WhenAll(TcpSocket.ReadFromTcpSocket(socket), TcpSocket.WriteToTcpSocket(socket));
            socket.Close();
        }

        Console.WriteLine("Done listening");
        //socket.Close();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

public static class TcpSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();

    public static async Task ReadFromTcpSocket(Socket socket)
    {
        Console.WriteLine("Reading from TCP socket");

        var bytes = new byte[socket.ReceiveBufferSize];
        var dataBuffer = new ArraySegment<byte>(bytes);
        int numberOfBytes;

        while ((numberOfBytes = await socket.ReceiveAsync(dataBuffer)) != 0)
        {
            if (numberOfBytes != bytes.Length)
                dataBuffer = dataBuffer[..numberOfBytes];
            await DataChannel.Writer.WriteAsync(dataBuffer);

            // await Task.Delay(5000);
            // Console.Write(Encoding.UTF8.GetString(dataBuffer));
        }
    }

    public static async Task WriteToTcpSocket(Socket socket)
    {
        Console.WriteLine("Writing to TCP socket");

        //can be removed since after writing socket can be closed
        // while (await WSocket.DataChannel.Reader.WaitToReadAsync())
        // {
        // var tcpData = await WSocket.DataChannel.Reader.ReadAsync();
        // socket.Send(tcpData, SocketFlags.None);
        // socket.Send(new byte[]{0});
        // await Task.Delay(2000);
        // }
        
        
        while (await WSocket.DataChannel.Reader.WaitToReadAsync())
        {
            var tcpData = await WSocket.DataChannel.Reader.ReadAsync();
            await socket.SendAsync(tcpData);
        }
    }
}

public static class WSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();
}

public class HandleWebSocketMiddleware : IMiddleware
{
    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        if (context.WebSockets.IsWebSocketRequest == false)
            await next(context);

        Console.WriteLine("New web socket comming");
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        while (webSocket.State == WebSocketState.Open)
        {
            await Task.WhenAll(ReadFromSocket(webSocket), WriteToSocket(webSocket));
        }

        Console.WriteLine("web socket closing");
    }

    public async Task ReadFromSocket(WebSocket webSocket)
    {
        Console.WriteLine("Reading from socket");

        var buffer = new byte[65536];
        var segment = new ArraySegment<byte>(buffer);
        WebSocketReceiveResult result;

        do
        {
            result = await webSocket.ReceiveAsync(segment, CancellationToken.None);
            if (result.Count != buffer.Length)
                segment = segment[..result.Count];
            Console.WriteLine(Encoding.UTF8.GetString(segment));
            await WSocket.DataChannel.Writer.WriteAsync(segment);
            
            
        } while (result.EndOfMessage == false);
    }

    public async Task WriteToSocket(WebSocket webSocket)
    {
        Console.WriteLine("Writing to socket");

        while (await TcpSocket.DataChannel.Reader.WaitToReadAsync())
        {
            var tcpData = await TcpSocket.DataChannel.Reader.ReadAsync();
            var isLastItem = TcpSocket.DataChannel.Reader.Count == 0;
            Console.Write(Encoding.UTF8.GetString(tcpData));
            var messageFlag = isLastItem
                ? WebSocketMessageFlags.EndOfMessage
                : WebSocketMessageFlags.None;

            await webSocket.SendAsync(tcpData, WebSocketMessageType.Binary, messageFlag, CancellationToken.None);

            // var tcpData = await TcpSocket.DataChannel.Reader.ReadAsync();
            // await webSocket.SendAsync(tcpData, WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage,
            //     CancellationToken.None);
            // ;
        }
    }
}