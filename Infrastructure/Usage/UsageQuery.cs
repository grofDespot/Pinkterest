using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Usage;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Usage;

public sealed class UsageQuery(ApplicationDbContext context, TimeProvider timeProvider) : IUsageQuery
{
    public async Task<Result<UsageSummary>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.Package)
            .Include(u => u.PendingPackage)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UsageSummary>(Error.NotFound("User"));
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var daily = await context.DailyUsages
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.UserId == userId && d.Date == today, cancellationToken);

        var totalBytesStored = await context.Photos
            .AsNoTracking()
            .Where(p => p.OwnerId == userId)
            .SumAsync(p => (long?)p.SizeBytes, cancellationToken) ?? 0L;

        return Result.Success(new UsageSummary(
            user.Package.Name,
            user.Package.MaxUploadSizeBytes,
            user.Package.DailyUploadLimit,
            user.Package.MaxTotalStorageBytes,
            daily?.UploadCount ?? 0,
            daily?.BytesUploaded ?? 0L,
            totalBytesStored,
            user.PendingPackage?.Name,
            user.PendingPackageEffectiveDate));
    }
}
