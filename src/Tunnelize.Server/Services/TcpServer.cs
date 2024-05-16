using System.Net;
using System.Net.Sockets;

namespace Tunnelize.Server.Services;

public static class TcpServer
{
    private static TcpListener _listener = null!;
    
    public static async void CreateTcpListener()
    {
        _listener = new TcpListener(IPAddress.Loopback, 8080);
        _listener.Start();

        while (true)
        {
            try
            {
                var socket = await _listener.AcceptSocketAsync();

                await TcpSocket.ReadFromTcpSocket(socket);
                await HandleWebSocketMiddleware.WriteToSocket(HandleWebSocketMiddleware.CurrentWebSocket);
                await HandleWebSocketMiddleware.ReadFromSocket(HandleWebSocketMiddleware.CurrentWebSocket);

                await TcpSocket.WriteToTcpSocket(socket);
                socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public static void Stop()
    {
        _listener.Stop();
        _listener.Dispose();
    }
}