using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Admin.Requests;
using Pinkterest.Application.Common.Mediation;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Packages;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Admin;

public sealed class UpdateUserHandler(ApplicationDbContext context)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateUserCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User"));
        }

        var packageExists = await context.Packages
            .AnyAsync(package => package.Id == request.PackageId, cancellationToken);

        if (!packageExists)
        {
            return Result.Failure(PackageErrors.Unknown);
        }

        user.DisplayName = request.DisplayName;
        user.PackageId = request.PackageId;
        user.PendingPackageId = null;
        user.PendingPackageEffectiveDate = null;

        if (request.ClearLockout)
        {
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
