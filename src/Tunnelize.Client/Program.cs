using Tunnelize.Client.Components;
using Tunnelize.Client.Routes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();

var app = builder.Build();
app.UseAntiforgery();
app.MapRoutes();
app.MapRazorComponents<App>();

app.Run();