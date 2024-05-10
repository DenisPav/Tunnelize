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
    Console.WriteLine("Reading from websocket");
    Console.WriteLine(Encoding.UTF8.GetString(segment));
    
    await WSocket.DataChannel.Writer.WriteAsync(segment);
    
    await CreateTcpSocket();
}

async Task WriteToSocket(WebSocket webSocket)
{
    while (await TcpSocket.DataChannel.Reader.WaitToReadAsync())
    {
        var tcpData = await TcpSocket.DataChannel.Reader.ReadAsync();
        var isLastItem = TcpSocket.DataChannel.Reader.Count == 0;
        // Console.WriteLine("------------------------------");
        // Console.Write(Encoding.UTF8.GetString(tcpData));
        // Console.WriteLine();
        // Console.WriteLine("------------------------------");
        var messageFlag = isLastItem
            ? WebSocketMessageFlags.EndOfMessage
            : WebSocketMessageFlags.None;
        
        Console.WriteLine("Writing to websocket");
        Console.WriteLine(Encoding.UTF8.GetString(tcpData));
        
        await webSocket.SendAsync(tcpData, WebSocketMessageType.Binary, messageFlag, CancellationToken.None);

        return;
        
        // if (isLastItem)
        //     return;
    }
}

async Task CreateTcpSocket()
{
    try
    {
        var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        await tcpSocket.ConnectAsync("127.0.0.1", 3000);

        // while (tcpSocket.Connected)
        // {
            await Task.WhenAll(TcpSocket.ReadFromTcpSocket(tcpSocket), TcpSocket.WriteToTcpSocket(tcpSocket));
        // }

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
        Console.WriteLine("Reading from local application");
        Console.WriteLine(Encoding.UTF8.GetString(dataBuffer));
        
        await DataChannel.Writer.WriteAsync(dataBuffer);
    }

    public static async Task WriteToTcpSocket(Socket socket)
    {
        while (await WSocket.DataChannel.Reader.WaitToReadAsync())
        {
            // var tcpData = await WSocket.DataChannel.Reader.ReadAsync();
            // await socket.SendAsync(tcpData);
            
            
            var tcpData = await WSocket.DataChannel.Reader.ReadAsync();
            
            Console.WriteLine("Writing to local application");
            Console.WriteLine(Encoding.UTF8.GetString(tcpData));
            
            await socket.SendAsync(tcpData);
            var isLastItem = WSocket.DataChannel.Reader.Count == 0;

            return;
            // if (isLastItem)
            //     return;
        }
    }
}