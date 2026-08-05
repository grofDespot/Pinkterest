using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Admin.Models;
using Pinkterest.Application.Admin.Requests;
using Pinkterest.Application.Common.Mediation;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Admin;

public sealed class GetUsersHandler(ApplicationDbContext context, TimeProvider timeProvider)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<AdminUserSummary>>
{
    public async Task<IReadOnlyList<AdminUserSummary>> HandleAsync(
        GetUsersQuery request,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var query = context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();

            query = query.Where(user =>
                user.DisplayName.ToLower().Contains(search) ||
                user.Email!.ToLower().Contains(search));
        }

        return await query
            .OrderBy(user => user.DisplayName)
            .Select(user => new AdminUserSummary(
                user.Id,
                user.DisplayName,
                user.Email!,
                user.Package.Name,
                user.Photos.Count,
                user.Photos.Sum(photo => (long?)photo.SizeBytes) ?? 0L,
                user.CreatedUtc,
                user.LockoutEnd != null && user.LockoutEnd > now))
            .ToListAsync(cancellationToken);
    }
}
