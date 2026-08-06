namespace Pinkterest.CrossCutting.Metrics;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MeasuredAttribute(string operation) : Attribute
{
    public string Operation { get; } = operation;
}
