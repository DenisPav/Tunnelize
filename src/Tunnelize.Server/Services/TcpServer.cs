using System.Net;
using System.Net.Sockets;

namespace Tunnelize.Server.Services;

public static class TcpServer
{
    private static TcpListener _listener = null!;
    private static CancellationTokenSource _cancellationTokenSource = null!;

    public static async void CreateTcpListener(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _listener = new TcpListener(IPAddress.Loopback, 8080);
        _listener.Start();

        while (_cancellationTokenSource.Token.IsCancellationRequested == false)
        {
            try
            {
                var socket = await _listener.AcceptSocketAsync(_cancellationTokenSource.Token);

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

                TcpSocket.Reset();
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
        _cancellationTokenSource.Cancel();
    }
}