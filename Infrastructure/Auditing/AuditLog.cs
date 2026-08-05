using System.Text.Json;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.Application.Common.Interfaces;
using Pinkterest.Domain.Entities;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Auditing;

public sealed class AuditLog(
    ApplicationDbContext context,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IAuditLog
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        context.AuditLog.Add(new AuditLogEntry
        {
            UserId = entry.UserId ?? currentUser.UserId,
            UserName = entry.UserName ?? currentUser.UserName ?? "anonymous",
            OccurredUtc = timeProvider.GetUtcNow(),
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            IpAddress = currentUser.IpAddress,
            Succeeded = entry.Succeeded,
            DetailsJson = entry.Details is null ? null : JsonSerializer.Serialize(entry.Details, SerializerOptions)
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
