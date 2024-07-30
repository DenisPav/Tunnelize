using Microsoft.EntityFrameworkCore;
using Tunnelize.Server.Codes;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Authentication;

public class AuthCodeCleanerBackgroundService(
    ILogger<CodeGenerator> log,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            log.LogInformation("Removing expired auth codes!");

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var currentDateTime = DateTime.UtcNow;
                var deletedCount = await context.Set<UserCode>().Where(x => x.Expiration < currentDateTime)
                    .ExecuteDeleteAsync(cancellationToken: stoppingToken);

                log.LogInformation("Removed [{deletedCount}] row/rows", deletedCount);
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}