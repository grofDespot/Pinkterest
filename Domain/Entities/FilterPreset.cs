using Pinkterest.Domain.Common;

namespace Pinkterest.Domain.Entities;

public class FilterPreset : Entity
{
    public Guid OwnerId { get; set; }

    public ApplicationUser Owner { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string DefinitionJson { get; set; } = "{}";

    public DateTimeOffset CreatedUtc { get; set; }
}
