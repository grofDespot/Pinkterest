using Pinkterest.Domain.Common;

namespace Pinkterest.Domain.Events;

public sealed record PhotoDetailsUpdatedEvent(
    Guid PhotoId,
    Guid EditorId,
    bool EditedByAdministrator,
    DateTimeOffset OccurredUtc) : IDomainEvent;
