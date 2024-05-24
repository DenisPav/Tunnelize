using Microsoft.EntityFrameworkCore;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Services;

namespace Tunnelize.Server;

public class StartupHostedService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<StartupHostedService> log)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("Performing startup tasks");
        log.LogInformation("Upgrading database");
        using var scope = serviceScopeFactory.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await db.Database.MigrateAsync(cancellationToken);
        
        log.LogInformation("Creating TCP listener");
        TcpServer.CreateTcpListener(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("Performing shutdown tasks");
        log.LogInformation("Stopping TCP listener");
        TcpServer.Stop();
        
        return Task.CompletedTask;
    }
}