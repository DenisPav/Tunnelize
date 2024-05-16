using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using Tunnelize.Server.Components;
using Tunnelize.Server.Routes;
using Tunnelize.Server.Services;

TcpServer.CreateTcpListener();

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
app.MapRoutes();
app.Run();