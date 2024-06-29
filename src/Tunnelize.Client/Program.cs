using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Channels;
using Tunnelize.Client.Components;
using Tunnelize.Client.Routes;





// CreateWebSocket();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();

var app = builder.Build();
app.UseAntiforgery();
app.MapRoutes();
app.MapRazorComponents<App>();

app.Run();

public static class WSocket
{
    public static readonly Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();
}