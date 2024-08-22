using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tunnelize.Server;
using Tunnelize.Server.Authentication;
using Tunnelize.Server.Components;
using Tunnelize.Server.Emails;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Routes;
using Tunnelize.Server.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DatabaseContext>(opts =>
    opts.UseSqlite("Data Source=./app.db;").EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));
builder.Services.AddWebSockets(_ => { });
builder.Services.AddAntiforgery(opts => { opts.Cookie.Name = "af"; });
builder.Services.AddRazorComponents();
builder.Services.AddAuthentication(Schemes.LoginCookie)
    .AddCookie(Schemes.IntermediateCookie, opts =>
    {
        opts.Cookie.Name = "ak";
        opts.LoginPath = "/login";
        opts.LogoutPath = "/logout";
        opts.ReturnUrlParameter = "ru";
        opts.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    })
    .AddCookie(Schemes.LoginCookie, opts =>
    {
        opts.Cookie.Name = "akc";
        opts.LoginPath = "/login";
        opts.LogoutPath = "/logout";
        opts.ReturnUrlParameter = "ru";
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHostedService<StartupHostedService>();
builder.Services.AddHostedService<AuthCodeCleanerBackgroundService>();
builder.Services.AddTunnelizeServer();
builder.Services.AddOptions<EmailSenderOptions>()
    .BindConfiguration("Email")
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<EmailSenderOptions>, EmailSenderOptionsValidator>();
builder.Services.AddSingleton<EmailSenderOptions>(sp => sp.GetRequiredService<IOptions<EmailSenderOptions>>().Value);
var app = builder.Build();

app.UseWebSockets();
app.UseMiddleware<HandleWebSocketMiddleware>();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorComponents<App>().RequireAuthorization(opts => opts.RequireAuthenticatedUser()
    .AddAuthenticationSchemes(Schemes.LoginCookie));
app.MapRoutes();
app.Run();