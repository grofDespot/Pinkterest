using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Admin.Models;
using Pinkterest.Application.Admin.Requests;
using Pinkterest.Application.Common.Mediation;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Admin;

public sealed class GetUserDetailHandler(ApplicationDbContext context)
    : IRequestHandler<GetUserDetailQuery, AdminUserDetail?>
{
    public Task<AdminUserDetail?> HandleAsync(
        GetUserDetailQuery request,
        CancellationToken cancellationToken = default) =>
        context.Users
            .AsNoTracking()
            .Where(user => user.Id == request.UserId)
            .Select(user => new AdminUserDetail(
                user.Id,
                user.DisplayName,
                user.Email!,
                user.PackageId,
                user.Package.Name,
                user.PendingPackage != null ? user.PendingPackage.Name : null,
                user.PendingPackageEffectiveDate,
                user.Photos.Count,
                user.Photos.Sum(photo => (long?)photo.SizeBytes) ?? 0L,
                user.CreatedUtc))
            .SingleOrDefaultAsync(cancellationToken);
}
