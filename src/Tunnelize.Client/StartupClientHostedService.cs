using Microsoft.EntityFrameworkCore;
using Tunnelize.Client.Persistence;

namespace Tunnelize.Client;

public class StartupClientHostedService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<StartupClientHostedService> log)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("Performing startup tasks");
        log.LogInformation("Upgrading database");
        using var scope = serviceScopeFactory.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("Performing shutdown tasks");
        
        return Task.CompletedTask;
    }
}