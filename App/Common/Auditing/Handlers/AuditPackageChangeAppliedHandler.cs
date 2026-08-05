using Pinkterest.Application.Common.Events;
using Pinkterest.Domain.Events;

namespace Pinkterest.Application.Common.Auditing.Handlers;

public sealed class AuditPackageChangeAppliedHandler(IAuditLog auditLog)
    : IDomainEventHandler<PackageChangeAppliedEvent>
{
    public Task HandleAsync(PackageChangeAppliedEvent domainEvent, CancellationToken cancellationToken = default) =>
        auditLog.RecordAsync(
            new AuditEntry(
                AuditActions.PackageChangeApplied,
                EntityType: nameof(Domain.Entities.ApplicationUser),
                EntityId: domainEvent.UserId.ToString(),
                Details: new { domainEvent.PackageId },
                UserId: domainEvent.UserId,
                UserName: "system"),
            cancellationToken);
}
