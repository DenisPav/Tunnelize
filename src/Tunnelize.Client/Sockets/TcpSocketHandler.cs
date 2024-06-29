using System.Net.Sockets;
using System.Threading.Channels;

namespace Tunnelize.Client.Sockets;

public class TcpSocketHandler
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
    
    public static async Task CreateTcpSocket()
    {
        try
        {
            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            await tcpSocket.ConnectAsync("127.0.0.1", 3000);
            await WriteToTcpSocket(tcpSocket);
            await ReadFromTcpSocket(tcpSocket);

            tcpSocket.Shutdown(SocketShutdown.Both);
            tcpSocket.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}