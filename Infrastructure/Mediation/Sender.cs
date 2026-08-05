using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Pinkterest.Application.Common.Mediation;

namespace Pinkterest.Infrastructure.Mediation;

public sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, object> Wrappers = new();

    public Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (HandlerWrapper<TResponse>)Wrappers.GetOrAdd(request.GetType(), requestType =>
        {
            var wrapperType = typeof(HandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));

            return Activator.CreateInstance(wrapperType)
                ?? throw new InvalidOperationException($"Could not create a handler wrapper for {requestType}.");
        });

        return wrapper.HandleAsync(request, serviceProvider, cancellationToken);
    }

    private abstract class HandlerWrapper<TResponse>
    {
        public abstract Task<TResponse> HandleAsync(
            IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    private sealed class HandlerWrapper<TRequest, TResponse> : HandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> HandleAsync(
            IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken) =>
            serviceProvider
                .GetRequiredService<IRequestHandler<TRequest, TResponse>>()
                .HandleAsync((TRequest)request, cancellationToken);
    }
}
