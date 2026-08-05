using Pinkterest.Domain.Common;

namespace Pinkterest.Domain.Events;

public sealed record PackageChangeRequestedEvent(
    Guid UserId,
    Guid CurrentPackageId,
    Guid TargetPackageId,
    DateOnly EffectiveDate,
    DateTimeOffset OccurredUtc) : IDomainEvent;
