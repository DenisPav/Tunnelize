using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
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
    var semaphore = new SemaphoreSlim(1);
    Console.WriteLine("Started TCP listener");
    while (true)
    {
        try
        {
            Console.WriteLine("New socket connected");

            await semaphore.WaitAsync();

            var socket = await listener.AcceptSocketAsync();

            // while (socket.Connected)
            // {


            await TcpSocket.ReadFromTcpSocket(socket);
            await HandleWebSocketMiddleware.WriteToSocket(HandleWebSocketMiddleware.CurrentWebSocket);
            await HandleWebSocketMiddleware.ReadFromSocket(HandleWebSocketMiddleware.CurrentWebSocket);

            await TcpSocket.WriteToTcpSocket(socket);
            // await socket.DisconnectAsync(true, CancellationToken.None);
            socket.Close();
            // }
            Console.WriteLine("Done listening");

            semaphore.Release();
            //socket.Close();
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

            Console.WriteLine("Reading from outside request");
            Console.WriteLine(Encoding.UTF8.GetString(dataBuffer));

            await DataChannel.Writer.WriteAsync(dataBuffer);

            return;
            // if (socket.Available == 0)
            //     return;

            // await Task.Delay(5000);
            // Console.Write(Encoding.UTF8.GetString(dataBuffer));
        }
    }

    public static async Task WriteToTcpSocket(Socket socket)
    {
        // Console.WriteLine("Writing to TCP socket");

        //can be removed since after writing socket can be closed
        // while (await WSocket.DataChannel.Reader.WaitToReadAsync())
        // {
        // var tcpData = await WSocket.DataChannel.Reader.ReadAsync();
        // socket.Send(tcpData, SocketFlags.None);
        // socket.Send(new byte[]{0});
        // await Task.Delay(2000);
        // }

        // while (await WSocket.DataChannel.Reader.WaitToReadAsync())
        var combinedData = new List<byte>();
        while (true)
        {
            var readSuccess = WSocket.DataChannel.Reader.TryRead(out var tcpData);
            if (readSuccess == false)
                break;

            combinedData.AddRange(tcpData);
            Console.WriteLine("Sending back to the outside response for request");
            // Console.WriteLine(Encoding.UTF8.GetString(tcpData));

            // await socket.SendAsync(tcpData);
            // var isLastItem = WSocket.DataChannel.Reader.Count == 0;
            //
            // if (isLastItem)
            //     return;
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

        Console.WriteLine("New web socket comming");
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        CurrentWebSocket = webSocket;
        while (webSocket.State == WebSocketState.Open)
        {
            await Task.Delay(Timeout.Infinite);
            // await WriteToSocket(webSocket);
            // await ReadFromSocket(webSocket);
            // await Task.WhenAll(ReadFromSocket(webSocket), );
        }

        Console.WriteLine("web socket closing");
    }

    public static async Task ReadFromSocket(WebSocket webSocket)
    {
        Console.WriteLine("Reading from socket");

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

        Console.WriteLine("Reading from client cli");
        Console.WriteLine(Encoding.UTF8.GetString(segment));

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

        // var tcpData = await TcpSocket.DataChannel.Reader.ReadAsync();
        // var isLastItem = TcpSocket.DataChannel.Reader.Count == 0;

        var tcpData = new ArraySegment<byte>(data.ToArray()); 
        Console.WriteLine("Writing to client cli");
        Console.WriteLine(Encoding.UTF8.GetString(tcpData));

        // var messageFlag = isLastItem
        //     ? WebSocketMessageFlags.EndOfMessage
        //     : WebSocketMessageFlags.None;
        
        await webSocket.SendAsync(tcpData, WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);    

        // var tcpData = await TcpSocket.DataChannel.Reader.ReadAsync();
        // await webSocket.SendAsync(tcpData, WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage,
        //     CancellationToken.None);
        // ;
    }
}