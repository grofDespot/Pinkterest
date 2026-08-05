using Pinkterest.Application.Common.Events;
using Pinkterest.Domain.Events;

namespace Pinkterest.Application.Common.Auditing.Handlers;

public sealed class AuditPhotoUploadedHandler(IAuditLog auditLog)
    : IDomainEventHandler<PhotoUploadedEvent>
{
    public Task HandleAsync(PhotoUploadedEvent domainEvent, CancellationToken cancellationToken = default) =>
        auditLog.RecordAsync(
            new AuditEntry(
                AuditActions.PhotoUpload,
                EntityType: nameof(Domain.Entities.Photo),
                EntityId: domainEvent.PhotoId.ToString(),
                Details: new
                {
                    domainEvent.OriginalFileName,
                    domainEvent.SizeBytes
                },
                UserId: domainEvent.OwnerId),
            cancellationToken);
}
