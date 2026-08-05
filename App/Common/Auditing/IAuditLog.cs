namespace Pinkterest.Application.Common.Auditing;

public interface IAuditLog
{
    Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
