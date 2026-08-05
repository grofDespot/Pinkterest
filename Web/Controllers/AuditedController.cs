using Microsoft.AspNetCore.Mvc;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.Application.Common.Results;

namespace Pinkterest.Web.Controllers;

public abstract class AuditedController(IAuditLog auditLog) : Controller
{
    protected abstract string AuditArea { get; }

    protected async Task<IActionResult> ExecuteAuditedAsync(
        string action,
        Func<Task<Result>> operation,
        Func<IActionResult> onSuccess,
        string? entityType = null,
        string? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var auditAction = $"{AuditArea}.{action}";
        Result result;

        try
        {
            result = await operation();
        }
        catch
        {
            await auditLog.RecordAsync(
                new AuditEntry(auditAction, entityType, entityId, Succeeded: false), cancellationToken);
            throw;
        }

        await auditLog.RecordAsync(
            new AuditEntry(auditAction, entityType, entityId, result.IsSuccess), cancellationToken);

        return result.IsSuccess ? onSuccess() : OnFailure(result.Error);
    }

    protected virtual IActionResult OnFailure(Error error)
    {
        TempData["AdminError"] = error.Message;
        return RedirectToAction("Index");
    }
}
