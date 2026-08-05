using Pinkterest.Application.Common.Events;
using Pinkterest.Domain.Events;

namespace Pinkterest.Application.Common.Auditing.Handlers;

public sealed class AuditUserRegisteredHandler(IAuditLog auditLog)
    : IDomainEventHandler<UserRegisteredEvent>
{
    public Task HandleAsync(UserRegisteredEvent domainEvent, CancellationToken cancellationToken = default) =>
        auditLog.RecordAsync(
            new AuditEntry(
                AuditActions.Register,
                EntityType: nameof(Domain.Entities.ApplicationUser),
                EntityId: domainEvent.UserId.ToString(),
                Details: new { domainEvent.PackageId },
                UserId: domainEvent.UserId,
                UserName: domainEvent.Email),
            cancellationToken);
}
