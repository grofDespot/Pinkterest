using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Pinkterest.Infrastructure.Observability;

namespace Pinkterest.Web.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddPinkterestObservability(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: PinkterestMeter.Name,
                serviceVersion: PinkterestMeter.Version))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(PinkterestMeter.Name)
                .AddView("pinkterest.operation.duration", new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = [5, 10, 25, 50, 100, 250, 500, 1000, 2500, 5000]
                })
                .AddPrometheusExporter());

        return services;
    }
}
