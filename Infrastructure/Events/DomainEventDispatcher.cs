using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pinkterest.Application.Common.Events;
using Pinkterest.Domain.Common;

namespace Pinkterest.Infrastructure.Events;

public sealed class DomainEventDispatcher(
    IServiceProvider serviceProvider,
    ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        foreach (var handler in serviceProvider.GetServices<IDomainEventHandler<TEvent>>())
        {
            try
            {
                await handler.HandleAsync(domainEvent, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Handler {Handler} failed while processing {Event}.",
                    handler.GetType().Name,
                    typeof(TEvent).Name);
            }
        }
    }
}
