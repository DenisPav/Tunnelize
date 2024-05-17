using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using Superpower;
using Tunnelize.Server.Parsers;

namespace Tunnelize.Server.Services;

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

            var decoded = Encoding.UTF8.GetString(dataBuffer);
            var result = HttpParsers.HostParser.TryParse(decoded);
            Console.WriteLine(result.Value.ToString());
            
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