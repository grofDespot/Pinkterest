using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Admin.Models;
using Pinkterest.Application.Admin.Requests;
using Pinkterest.Application.Common.Mediation;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Admin;

public sealed class GetAuditLogHandler(ApplicationDbContext context)
    : IRequestHandler<GetAuditLogQuery, AuditLogPage>
{
    private const int MaxPageSize = 200;

    public async Task<AuditLogPage> HandleAsync(
        GetAuditLogQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = context.AuditLog.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim().ToLower();
            query = query.Where(entry => entry.Action.ToLower().Contains(action));
        }

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            var userName = request.UserName.Trim().ToLower();
            query = query.Where(entry => entry.UserName.ToLower().Contains(userName));
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(entry => entry.OccurredUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(entry => new AuditLogItem(
                entry.Id,
                entry.OccurredUtc,
                entry.UserName,
                entry.Action,
                entry.EntityType,
                entry.EntityId,
                entry.IpAddress,
                entry.Succeeded,
                entry.DetailsJson))
            .ToListAsync(cancellationToken);

        return new AuditLogPage(items, total, page, pageSize);
    }
}
