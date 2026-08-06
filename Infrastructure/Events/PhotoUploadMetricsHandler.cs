using System.Diagnostics.Metrics;
using Pinkterest.Application.Common.Events;
using Pinkterest.Domain.Events;
using Pinkterest.Infrastructure.Interception;

namespace Pinkterest.Infrastructure.Events;

public sealed class PhotoUploadMetricsHandler : IDomainEventHandler<PhotoUploadedEvent>
{
    private static readonly Meter Meter = new(MetricsInterceptor.MeterName, "1.0.0");

    private static readonly Counter<long> PhotosUploaded = Meter.CreateCounter<long>(
        "pinkterest.photos.uploaded", "photos", "Photos successfully stored.");

    private static readonly Histogram<long> UploadedBytes = Meter.CreateHistogram<long>(
        "pinkterest.photos.uploaded.bytes", "By", "Size of stored photos.");

    public Task HandleAsync(PhotoUploadedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        PhotosUploaded.Add(1);
        UploadedBytes.Record(domainEvent.SizeBytes);

        return Task.CompletedTask;
    }
}
