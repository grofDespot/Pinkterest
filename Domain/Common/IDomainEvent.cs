namespace Pinkterest.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredUtc { get; }
}
