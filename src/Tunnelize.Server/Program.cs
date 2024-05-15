using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using Tunnelize.Server.Components;
using Tunnelize.Server.Components.ApiKeys;

// CreateTcpListener();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebSockets(_ => { });
builder.Services.AddAntiforgery(opts =>
{
    opts.Cookie.Name = "af";
});
builder.Services.AddRazorComponents();
builder.Services.AddAuthentication()
    .AddCookie(opts =>
    {
        opts.Cookie.Name = "ak";
        opts.LoginPath = "/login";
        opts.LogoutPath = "/logout";
        opts.ReturnUrlParameter = "ru";
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<HandleWebSocketMiddleware>();
var app = builder.Build();

app.UseWebSockets();
app.UseMiddleware<HandleWebSocketMiddleware>();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorComponents<App>();
var authRoutes = app.MapGroup("authentication");
authRoutes.MapPost("/login", async (
    [FromForm] LoginRequest request,
    HttpContext context,
    [FromServices] IAuthenticationService authenticationService) =>
{
    context.Response.Headers.Append("HX-Redirect", "/dashboard");
    var claims = new[]
    {
        new Claim(ClaimTypes.Email, "denis.pav@hotmail.com"),
        new Claim(ClaimTypes.Name, "denis.pav@hotmail.com")
    };
    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(claimsIdentity);
    await authenticationService.SignInAsync(context, CookieAuthenticationDefaults.AuthenticationScheme, principal,
        null);

    return TypedResults.Empty;
});
authRoutes.MapPost("/logout", async (
    HttpContext context,
    [FromServices] IAuthenticationService authenticationService) =>
{
    context.Response.Headers.Append("HX-Redirect", "/login");
    await authenticationService.SignOutAsync(context, CookieAuthenticationDefaults.AuthenticationScheme, null);

    return TypedResults.Empty;
});
app.MapGet("/api-keys/create", (HttpContext context) =>
{
    context.Response.Headers.Append("HX-Redirect", "/create");
    return TypedResults.Empty;
});
app.MapPost("/api-keys/create", (HttpContext context) =>
{
    context.Response.Headers.Append("HX-Redirect", "/dashboard");
    return TypedResults.Empty;
});
app.MapDelete("/api-keys/delete/{id}", (HttpContext context) =>
{
    return TypedResults.Empty;
});
app.Run();

async void CreateTcpListener()
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

public static class WSocket
{
    public static Channel<ArraySegment<byte>> DataChannel = Channel.CreateUnbounded<ArraySegment<byte>>();
}

public class HandleWebSocketMiddleware : IMiddleware
{
    public static WebSocket CurrentWebSocket;

    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        if (context.WebSockets.IsWebSocketRequest == false)
        {
            await next(context);
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        CurrentWebSocket = webSocket;
        while (webSocket.State == WebSocketState.Open)
        {
            await Task.Delay(Timeout.Infinite);
        }
    }

    public static async Task ReadFromSocket(WebSocket webSocket)
    {
        var buffer = new byte[65536];
        var segment = new ArraySegment<byte>(buffer);
        WebSocketReceiveResult result;
        var data = new List<byte>();
        do
        {
            result = await webSocket.ReceiveAsync(segment, CancellationToken.None);
            if (result.Count != buffer.Length)
                segment = segment[..result.Count];

            data.AddRange(segment);
        } while (result.EndOfMessage == false);

        segment = new ArraySegment<byte>(data.ToArray());

        await WSocket.DataChannel.Writer.WriteAsync(segment);
    }

    public static async Task WriteToSocket(WebSocket webSocket)
    {
        var data = new List<byte>();
        while (true)
        {
            var readSuccess = TcpSocket.DataChannel.Reader.TryRead(out var readData);
            if (readSuccess == false)
                break;

            data.AddRange(readData);
        }

        var tcpData = new ArraySegment<byte>(data.ToArray());
        await webSocket.SendAsync(
            tcpData,
            WebSocketMessageType.Binary,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None);
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}