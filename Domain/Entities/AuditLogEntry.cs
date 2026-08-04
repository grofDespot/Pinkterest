namespace Pinkterest.Domain.Entities;

public class AuditLogEntry
{
    public long Id { get; set; }

    public Guid? UserId { get; set; }

    public string UserName { get; set; } = "anonymous";

    public DateTimeOffset OccurredUtc { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    public string? IpAddress { get; set; }

    public bool Succeeded { get; set; }

    public string? DetailsJson { get; set; }
}
