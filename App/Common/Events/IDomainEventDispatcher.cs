using Pinkterest.Domain.Common;

namespace Pinkterest.Application.Common.Events;

public interface IDomainEventDispatcher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}
