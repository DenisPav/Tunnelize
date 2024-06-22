using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using Superpower;
using Tunnelize.Server.Parsers;

namespace Tunnelize.Server.Services;

public static class TcpSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();

    public static async Task<string> ReadFromTcpSocket(Socket socket)
    {
        var bytes = new byte[socket.ReceiveBufferSize];
        var firstSegmentBytes = new byte[socket.ReceiveBufferSize];
        var dataBuffer = new ArraySegment<byte>(bytes);
        ArraySegment<byte>? firstSegment = null;
        int numberOfBytes;
        
        while ((numberOfBytes = await socket.ReceiveAsync(dataBuffer)) != 0)
        {
            if (numberOfBytes != bytes.Length)
                dataBuffer = dataBuffer[..numberOfBytes];

            if (firstSegment is null)
            {
                firstSegment = new ArraySegment<byte>(firstSegmentBytes);
                dataBuffer.CopyTo(firstSegment.Value);
            }
            
            await DataChannel.Writer.WriteAsync(dataBuffer);
            
            if (numberOfBytes != bytes.Length)
            {
                break;
            }

            bytes = new byte[socket.ReceiveBufferSize];
            dataBuffer = new ArraySegment<byte>(bytes);
        }
        
        var decoded = Encoding.UTF8.GetString(firstSegment!.Value);
        DataChannel.Reader.TryPeek(out var peeked);
        var decoded2 = Encoding.UTF8.GetString(peeked);
        var result = HttpParsers.HostParser.TryParse(decoded);
        return result.Value.ToString();
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