using Microsoft.EntityFrameworkCore;
using Tunnelize.Client;
using Tunnelize.Client.Components;
using Tunnelize.Client.Persistence;
using Tunnelize.Client.Routes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DatabaseContext>(opts =>
    opts.UseSqlite("Data Source=./app.db;").EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));
builder.Services.AddRazorComponents();
builder.Services.AddHostedService<StartupClientHostedService>();

var app = builder.Build();
app.UseAntiforgery();
app.MapRoutes();
app.MapRazorComponents<App>();

app.Run();