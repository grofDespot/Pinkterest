using Pinkterest.Domain.Common;

namespace Pinkterest.Application.Common.Events;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
