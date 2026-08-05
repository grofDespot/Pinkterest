using Pinkterest.Application.Common.Events;
using Pinkterest.Domain.Events;

namespace Pinkterest.Application.Common.Auditing.Handlers;

public sealed class AuditPackageChangeRequestedHandler(IAuditLog auditLog)
    : IDomainEventHandler<PackageChangeRequestedEvent>
{
    public Task HandleAsync(PackageChangeRequestedEvent domainEvent, CancellationToken cancellationToken = default) =>
        auditLog.RecordAsync(
            new AuditEntry(
                AuditActions.PackageChangeRequested,
                EntityType: nameof(Domain.Entities.ApplicationUser),
                EntityId: domainEvent.UserId.ToString(),
                Details: new
                {
                    domainEvent.CurrentPackageId,
                    domainEvent.TargetPackageId,
                    EffectiveDate = domainEvent.EffectiveDate.ToString("yyyy-MM-dd")
                },
                UserId: domainEvent.UserId),
            cancellationToken);
}
