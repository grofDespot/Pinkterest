using Pinkterest.Application.Common.Events;
using Pinkterest.Domain.Events;

namespace Pinkterest.Application.Common.Auditing.Handlers;

public sealed class AuditPhotoDetailsUpdatedHandler(IAuditLog auditLog)
    : IDomainEventHandler<PhotoDetailsUpdatedEvent>
{
    public Task HandleAsync(PhotoDetailsUpdatedEvent domainEvent, CancellationToken cancellationToken = default) =>
        auditLog.RecordAsync(
            new AuditEntry(
                AuditActions.PhotoEdit,
                EntityType: nameof(Domain.Entities.Photo),
                EntityId: domainEvent.PhotoId.ToString(),
                Details: new { domainEvent.EditedByAdministrator },
                UserId: domainEvent.EditorId),
            cancellationToken);
}
