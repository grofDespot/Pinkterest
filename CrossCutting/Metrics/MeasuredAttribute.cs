namespace Pinkterest.CrossCutting.Metrics;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MeasuredAttribute(string metricName) : Attribute
{
    public string MetricName { get; } = metricName;
}
