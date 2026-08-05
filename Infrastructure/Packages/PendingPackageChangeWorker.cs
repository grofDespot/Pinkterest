using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pinkterest.Application.Packages;

namespace Pinkterest.Infrastructure.Packages;

public sealed class PendingPackageChangeWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<PendingPackageChangeWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<IPackageChangeService>();
                await service.ApplyDueChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to apply scheduled package changes.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
