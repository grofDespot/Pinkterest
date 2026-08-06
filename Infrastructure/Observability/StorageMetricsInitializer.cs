using Microsoft.Extensions.Hosting;

namespace Pinkterest.Infrastructure.Observability;

public sealed class StorageMetricsInitializer(StorageMetrics metrics) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = metrics;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
