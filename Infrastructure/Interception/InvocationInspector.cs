using System.Reflection;
using Castle.DynamicProxy;
using Pinkterest.Application.Common.Results;

namespace Pinkterest.Infrastructure.Interception;

internal static class InvocationInspector
{
    public static TAttribute? AttributeOn<TAttribute>(IInvocation invocation) where TAttribute : Attribute =>
        (invocation.MethodInvocationTarget ?? invocation.Method).GetCustomAttribute<TAttribute>();

    public static bool Succeeded(object? returnValue) =>
        returnValue is not Result result || result.IsSuccess;

    public static string? EntityIdOf(object? returnValue) => returnValue switch
    {
        Result<Guid> { IsSuccess: true } guid => guid.Value.ToString(),
        Result<int> { IsSuccess: true } count => count.Value.ToString(),
        _ => null
    };
}
