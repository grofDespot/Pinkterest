using Microsoft.Extensions.Logging;
using Pinkterest.Application.Common.Events;
using Pinkterest.Domain.Events;

namespace Pinkterest.Infrastructure.Events;

public sealed class LogPhotoUploadedHandler(ILogger<LogPhotoUploadedHandler> logger)
    : IDomainEventHandler<PhotoUploadedEvent>
{
    public Task HandleAsync(PhotoUploadedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Photo {PhotoId} ({SizeBytes} bytes) uploaded by {OwnerId}.",
            domainEvent.PhotoId,
            domainEvent.SizeBytes,
            domainEvent.OwnerId);

        return Task.CompletedTask;
    }
}
