using System.Net;
using System.Net.Sockets;

namespace Tunnelize.Server.Services;

public static class TcpServer
{
    public static async void CreateTcpListener()
    {
        var listener = new TcpListener(IPAddress.Loopback, 8080);
        listener.Start();

        while (true)
        {
            try
            {
                var socket = await listener.AcceptSocketAsync();

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
}