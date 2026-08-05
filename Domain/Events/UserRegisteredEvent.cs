using Pinkterest.Domain.Common;

namespace Pinkterest.Domain.Events;

public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    Guid PackageId,
    DateTimeOffset OccurredUtc) : IDomainEvent;
