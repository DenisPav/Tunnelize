using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Server;
using Tunnelize.Server.Components;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Routes;
using Tunnelize.Server.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DatabaseContext>(opts => opts.UseSqlite("Data Source=./app.db;"));
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
builder.Services.AddHostedService<StartupHostedService>();
builder.Services.AddScoped<HandleWebSocketMiddleware>();
var app = builder.Build();

app.UseWebSockets();
app.UseMiddleware<HandleWebSocketMiddleware>();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorComponents<App>().RequireAuthorization(opts => opts.RequireAuthenticatedUser());
app.MapRoutes();
app.Run();