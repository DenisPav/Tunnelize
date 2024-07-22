using System.Net;
using System.Net.Sockets;

namespace Tunnelize.Server.Services;

public static class TcpServer
{
    private static TcpListener _listener = null!;
    
    public static async void CreateTcpListener(CancellationToken cancellationToken)
    {
        _listener = new TcpListener(IPAddress.Loopback, 8080);
        _listener.Start();

        while (cancellationToken.IsCancellationRequested == false)
        {
            try
            {
                var socket = await _listener.AcceptSocketAsync(cancellationToken);

                var hostName = await TcpSocket.ReadFromTcpSocket(socket);
                var dotIndex = hostName.IndexOf('.');
                var wildCardDomain = hostName[..dotIndex];

                HandleWebSocketMiddleware.WebSocketMap.TryGetValue(wildCardDomain, out var webSocket);

                if (webSocket is not null)
                {
                    await HandleWebSocketMiddleware.WriteToSocket(webSocket);
                    await HandleWebSocketMiddleware.ReadFromSocket(webSocket);

                    await TcpSocket.WriteToTcpSocket(socket);    
                }
                
                socket.Close();
            }
            catch
            {
                //TODO: log
            }
        }
    }

    public static void Stop()
    {
        _listener.Stop();
        _listener.Dispose();
    }
}