using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Server;
using Tunnelize.Server.Components;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Routes;
using Tunnelize.Server.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DatabaseContext>(opts =>
    opts.UseSqlite("Data Source=./app.db;").EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));
builder.Services.AddWebSockets(_ => { });
builder.Services.AddAntiforgery(opts => { opts.Cookie.Name = "af"; });
builder.Services.AddRazorComponents();
builder.Services.AddAuthentication("loginCookie")
    .AddCookie("intermediateCookie", opts =>
    {
        opts.Cookie.Name = "ak";
        opts.LoginPath = "/login";
        opts.LogoutPath = "/logout";
        opts.ReturnUrlParameter = "ru";
        opts.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    })
    .AddCookie("loginCookie", opts =>
    {
        opts.Cookie.Name = "akc";
        opts.LoginPath = "/login";
        opts.LogoutPath = "/logout";
        opts.ReturnUrlParameter = "ru";
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHostedService<StartupHostedService>();
builder.Services.AddTunnelizeServer();
var app = builder.Build();

app.UseWebSockets();
app.UseMiddleware<HandleWebSocketMiddleware>();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorComponents<App>().RequireAuthorization(opts => opts.RequireAuthenticatedUser()
    .AddAuthenticationSchemes("loginCookie"));
app.MapRoutes();
app.Run();