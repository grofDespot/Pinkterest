using Castle.DynamicProxy;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.CrossCutting.Auditing;

namespace Pinkterest.Infrastructure.Interception;

public sealed class AuditInterceptor(IAuditLog auditLog) : AsyncInterceptorBase
{
    protected override async Task InterceptAsync(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task> proceed)
    {
        var audited = InvocationInspector.AttributeOn<AuditedAttribute>(invocation);

        if (audited is null)
        {
            await proceed(invocation, proceedInfo);
            return;
        }

        try
        {
            await proceed(invocation, proceedInfo);
            await RecordAsync(audited, succeeded: true, entityId: null);
        }
        catch
        {
            await RecordAsync(audited, succeeded: false, entityId: null);
            throw;
        }
    }

    protected override async Task<TResult> InterceptAsync<TResult>(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var audited = InvocationInspector.AttributeOn<AuditedAttribute>(invocation);

        if (audited is null)
        {
            return await proceed(invocation, proceedInfo);
        }

        try
        {
            var returnValue = await proceed(invocation, proceedInfo);

            await RecordAsync(
                audited,
                InvocationInspector.Succeeded(returnValue),
                InvocationInspector.EntityIdOf(returnValue));

            return returnValue;
        }
        catch
        {
            await RecordAsync(audited, succeeded: false, entityId: null);
            throw;
        }
    }

    private Task RecordAsync(AuditedAttribute audited, bool succeeded, string? entityId) =>
        auditLog.RecordAsync(new AuditEntry(
            audited.Action,
            audited.EntityType,
            entityId,
            succeeded));
}
