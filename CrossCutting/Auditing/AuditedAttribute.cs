namespace Pinkterest.CrossCutting.Auditing;

[AttributeUsage(AttributeTargets.Method)]
public sealed class AuditedAttribute(string action) : Attribute
{
    public string Action { get; } = action;

    public string? EntityType { get; init; }
}
