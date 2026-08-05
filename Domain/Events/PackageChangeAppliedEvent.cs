using Pinkterest.Domain.Common;

namespace Pinkterest.Domain.Events;

public sealed record PackageChangeAppliedEvent(
    Guid UserId,
    Guid PackageId,
    DateTimeOffset OccurredUtc) : IDomainEvent;
