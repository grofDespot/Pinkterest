using System.Diagnostics;
using System.Diagnostics.Metrics;
using Castle.DynamicProxy;
using Pinkterest.CrossCutting.Metrics;
using Pinkterest.Infrastructure.Observability;

namespace Pinkterest.Infrastructure.Interception;

public sealed class MetricsInterceptor : AsyncInterceptorBase
{
    private static readonly Meter Meter = new(PinkterestMeter.Name, PinkterestMeter.Version);

    private static readonly Histogram<double> OperationDuration = Meter.CreateHistogram<double>(
        "pinkterest.operation.duration", "ms", "How long an audited business operation takes.");

    private static readonly Counter<long> OperationOutcomes = Meter.CreateCounter<long>(
        "pinkterest.operation.outcomes", "operations", "Business operations by name and outcome.");

    protected override async Task InterceptAsync(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task> proceed)
    {
        var measured = InvocationInspector.AttributeOn<MeasuredAttribute>(invocation);

        if (measured is null)
        {
            await proceed(invocation, proceedInfo);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await proceed(invocation, proceedInfo);
            Record(measured.Operation, stopwatch, "success");
        }
        catch
        {
            Record(measured.Operation, stopwatch, "faulted");
            throw;
        }
    }

    protected override async Task<TResult> InterceptAsync<TResult>(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var measured = InvocationInspector.AttributeOn<MeasuredAttribute>(invocation);

        if (measured is null)
        {
            return await proceed(invocation, proceedInfo);
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var returnValue = await proceed(invocation, proceedInfo);

            Record(
                measured.Operation,
                stopwatch,
                InvocationInspector.Succeeded(returnValue) ? "success" : "rejected");

            return returnValue;
        }
        catch
        {
            Record(measured.Operation, stopwatch, "faulted");
            throw;
        }
    }

    private static void Record(string operation, Stopwatch stopwatch, string outcome)
    {
        stopwatch.Stop();

        var tags = new TagList
        {
            { "operation", operation },
            { "outcome", outcome }
        };

        OperationDuration.Record(stopwatch.Elapsed.TotalMilliseconds, tags);
        OperationOutcomes.Add(1, tags);
    }
}
