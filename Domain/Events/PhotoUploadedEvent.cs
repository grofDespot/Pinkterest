using Pinkterest.Domain.Common;

namespace Pinkterest.Domain.Events;

public sealed record PhotoUploadedEvent(
    Guid PhotoId,
    Guid OwnerId,
    string OriginalFileName,
    long SizeBytes,
    DateTimeOffset OccurredUtc) : IDomainEvent;
