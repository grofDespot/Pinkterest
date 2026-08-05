namespace Pinkterest.Application.Common.Auditing;

public sealed record AuditEntry(
    string Action,
    string? EntityType = null,
    string? EntityId = null,
    bool Succeeded = true,
    object? Details = null,
    Guid? UserId = null,
    string? UserName = null);
