namespace Pinkterest.Application.Admin.Models;

public sealed record AuditLogItem(
    long Id,
    DateTimeOffset OccurredUtc,
    string UserName,
    string Action,
    string? EntityType,
    string? EntityId,
    string? IpAddress,
    bool Succeeded,
    string? DetailsJson);
