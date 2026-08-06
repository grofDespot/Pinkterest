using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace Pinkterest.Infrastructure.Interception;

public static class InterceptionServiceCollectionExtensions
{
    private static readonly ProxyGenerator Generator = new();

    public static IServiceCollection AddInterceptedScoped<TService, TImplementation>(
        this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddScoped<TImplementation>();

        services.AddScoped(provider =>
        {
            var target = provider.GetRequiredService<TImplementation>();
            var interceptors = provider.GetServices<IAsyncInterceptor>().ToArray();

            return interceptors.Length == 0
                ? target
                : Generator.CreateInterfaceProxyWithTargetInterface<TService>(target, interceptors);
        });

        return services;
    }
}
