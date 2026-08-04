using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Packages;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Packages;

public sealed class PackageCatalog(ApplicationDbContext context) : IPackageCatalog
{
    public async Task<IReadOnlyList<PackageDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Packages
            .AsNoTracking()
            .OrderBy(p => p.Tier)
            .Select(p => new PackageDto(
                p.Id,
                p.Name,
                p.MaxUploadSizeBytes,
                p.DailyUploadLimit,
                p.MaxTotalStorageBytes,
                p.MonthlyPrice))
            .ToListAsync(cancellationToken);
}
