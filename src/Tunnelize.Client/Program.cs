using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;

async void CreateWebSocket()
{
    try
    {
        var webSocket = new ClientWebSocket();
        // webSocket.Options.SetRequestHeader("x-tunnelize-key", "73441f56-7751-440e-a26c-0543a03ca9ad");
        webSocket.Options.SetRequestHeader("x-tunnelize-key", "2ddb329c-e17d-4b75-bc48-698c17e1e407");
        webSocket.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        // webSocket.Options.SetBuffer(int.MaxValue, int.MaxValue);
        // var serverLocation = new Uri("wss://tunnelize.xyz");
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
    // await webSocket.ReceiveAsync(segment, CancellationToken.None);

    WebSocketReceiveResult result;
    // var data = new List<byte>();
    do
    {
        var buffer = new byte[65536];
        var segment = new ArraySegment<byte>(buffer);
        result = await webSocket.ReceiveAsync(segment, CancellationToken.None);
        if (result.Count != buffer.Length)
            segment = segment[..result.Count];

        // var data = Encoding.UTF8.GetString(segment);
        // Console.WriteLine(data);

        await WSocket.DataChannel.Writer.WriteAsync(segment);
    } while (result.EndOfMessage == false);
    // segment = new ArraySegment<byte>(data.ToArray());

    // await WSocket.DataChannel.Writer.WriteAsync(segment);÷

    await CreateTcpSocket();
}

async Task WriteToSocket(WebSocket webSocket)
{
    // await TcpSocket.DataChannel.Reader.WaitToReadAsync();
    //
    // var tcpData = await TcpSocket.DataChannel.Reader.ReadAsync();
    // var isLastItem = TcpSocket.DataChannel.Reader.Count == 0;
    // var messageFlag = isLastItem
    //     ? WebSocketMessageFlags.EndOfMessage
    //     : WebSocketMessageFlags.None;
    //
    // await webSocket.SendAsync(tcpData, WebSocketMessageType.Binary, messageFlag, CancellationToken.None);

    var totalItems = TcpSocket.DataChannel.Reader.Count;

    for (var i = 0; i < totalItems; i++)
    {
        var readSuccess = TcpSocket.DataChannel.Reader.TryRead(out var readData);

        // if (readSuccess == false)
        //     break;

        var flag = i == totalItems - 1
            ? WebSocketMessageFlags.EndOfMessage
            : WebSocketMessageFlags.None;
        var tcpData = readData;
        
        // Console.Write(Encoding.UTF8.GetString(tcpData));
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
        // tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
        
        // tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
        // tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 2);
        // tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2);
        // tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        
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
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();
}

public static class TcpSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();

    public static async Task ReadFromTcpSocket(Socket socket)
    {
        //TODO: now this is also wrong the same way that sending was on server side scope buffer to while
        //TODO: and reset it on each run then follow that back up on server side
        var bytes = new byte[socket.ReceiveBufferSize];
        var dataBuffer = new ArraySegment<byte>(bytes);
        int numberOfBytes = 0;
        // var finalData = new List<byte>();
        
        try
        {
            while ((numberOfBytes = socket.Receive(dataBuffer)) != 0)
            {
                Console.WriteLine(numberOfBytes);
                
                if (numberOfBytes != bytes.Length)
                {
                    dataBuffer = dataBuffer[..numberOfBytes];
                }
                
                await DataChannel.Writer.WriteAsync(dataBuffer);
                Console.Write(Encoding.UTF8.GetString(dataBuffer));

                //document this, try with async stuff and also add this to configuration
                if (!socket.Poll(10000, SelectMode.SelectRead))
                    break;
                
                
                bytes = new byte[socket.ReceiveBufferSize];
                dataBuffer = new ArraySegment<byte>(bytes);
            }
            
            Console.WriteLine(numberOfBytes);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        Console.WriteLine("--------CLOSING--------");
        // Console.WriteLine();
        //
        // if (finalData.Count == 0)
        //     return;
        //
        // dataBuffer = new ArraySegment<byte>(finalData.ToArray());
        // await DataChannel.Writer.WriteAsync(dataBuffer);
    }

    public static async Task WriteToTcpSocket(Socket socket)
    {
        await WSocket.DataChannel.Reader.WaitToReadAsync();
        var totalItems = WSocket.DataChannel.Reader.Count;
        var allBufferData = new List<ArraySegment<byte>>();

        for (var i = 0; i < totalItems; i++)
        {
            var readSuccess = WSocket.DataChannel.Reader.TryRead(out var readData);

            // if (readSuccess == false)
            //     break;

            var flag = i == totalItems - 1
                ? SocketFlags.None
                : SocketFlags.Partial;

            //seems to be something with the flags

            var tcpData = readData;
            // Console.WriteLine("-----------------");
            var data = Encoding.UTF8.GetString(tcpData);
            // Console.WriteLine(data);
            // Console.WriteLine("-----------------");
            await socket.SendAsync(tcpData);
            // socket.Send(tcpData);
            // allBufferData.Add(readData);
        }

        // allBufferData.Reverse();
        // var totalData = allBufferData.SelectMany(x => x).Chunk(1024);

        // for (var i = 0; i < totalData.Count(); i++)
        // {
        //     var currentData = totalData.ElementAt(i);
        //     var segment = new ArraySegment<byte>(currentData);
        //
        //     Console.WriteLine(Encoding.UTF8.GetString(segment));
        //     
        //     await socket.SendAsync(segment);
        // }

        // await socket.SendAsync(allBufferData, SocketFlags.Partial);
    }
}