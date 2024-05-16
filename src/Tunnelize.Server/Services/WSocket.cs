using System.Threading.Channels;

namespace Tunnelize.Server.Services;

public static class WSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();
}