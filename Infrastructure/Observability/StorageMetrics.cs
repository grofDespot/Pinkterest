using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Observability;

public sealed class StorageMetrics : IDisposable
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StorageMetrics> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly Meter _meter;
    private readonly Lock _gate = new();

    private DateTimeOffset _refreshedUtc = DateTimeOffset.MinValue;
    private long _bytesStored;
    private long _photoCount;
    private long _userCount;

    public StorageMetrics(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        IMeterFactory meterFactory,
        ILogger<StorageMetrics> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
        _meter = meterFactory.Create(PinkterestMeter.Name, PinkterestMeter.Version);

        _meter.CreateObservableGauge(
            "pinkterest.storage.bytes.used",
            () => Snapshot().BytesStored,
            "By",
            "Total bytes of stored photos.");

        _meter.CreateObservableGauge(
            "pinkterest.photos.total",
            () => Snapshot().PhotoCount,
            "photos",
            "Photos currently visible in the gallery.");

        _meter.CreateObservableGauge(
            "pinkterest.users.total",
            () => Snapshot().UserCount,
            "users",
            "Registered accounts.");
    }

    private (long BytesStored, long PhotoCount, long UserCount) Snapshot()
    {
        lock (_gate)
        {
            var now = _timeProvider.GetUtcNow();

            if (now - _refreshedUtc < CacheDuration)
            {
                return (_bytesStored, _photoCount, _userCount);
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                _bytesStored = context.Photos.Sum(photo => (long?)photo.SizeBytes) ?? 0L;
                _photoCount = context.Photos.LongCount();
                _userCount = context.Users.LongCount();
                _refreshedUtc = now;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Could not refresh storage metrics; serving the previous values.");
            }

            return (_bytesStored, _photoCount, _userCount);
        }
    }

    public void Dispose() => _meter.Dispose();
}
