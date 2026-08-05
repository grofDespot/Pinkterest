using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Admin.Models;
using Pinkterest.Application.Admin.Requests;
using Pinkterest.Application.Common.Mediation;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Admin;

public sealed class GetAdminStatisticsHandler(ApplicationDbContext context, TimeProvider timeProvider)
    : IRequestHandler<GetAdminStatisticsQuery, AdminStatistics>
{
    private const int TrendDays = 14;

    public async Task<AdminStatistics> HandleAsync(
        GetAdminStatisticsQuery request,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var since = today.AddDays(-TrendDays);

        var userCount = await context.Users.CountAsync(cancellationToken);
        var photoCount = await context.Photos.CountAsync(cancellationToken);

        var totalBytes = await context.Photos
            .SumAsync(photo => (long?)photo.SizeBytes, cancellationToken) ?? 0L;

        var uploadsToday = await context.DailyUsages
            .Where(usage => usage.Date == today)
            .SumAsync(usage => (int?)usage.UploadCount, cancellationToken) ?? 0;

        var distributionRows = await context.Users
            .GroupBy(user => user.Package.Name)
            .Select(group => new { PackageName = group.Key, UserCount = group.Count() })
            .ToListAsync(cancellationToken);

        var trendRows = await context.DailyUsages
            .Where(usage => usage.Date >= since)
            .GroupBy(usage => usage.Date)
            .Select(group => new { Date = group.Key, Uploads = group.Sum(usage => usage.UploadCount) })
            .ToListAsync(cancellationToken);

        var uploaderRows = await context.Users
            .Select(user => new
            {
                user.DisplayName,
                PhotoCount = user.Photos.Count,
                BytesStored = user.Photos.Sum(photo => (long?)photo.SizeBytes) ?? 0L
            })
            .OrderByDescending(uploader => uploader.PhotoCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new AdminStatistics(
            userCount,
            photoCount,
            totalBytes,
            uploadsToday,
            [.. distributionRows.Select(row => new PackageDistribution(row.PackageName, row.UserCount))],
            [.. trendRows.OrderBy(row => row.Date).Select(row => new DailyUploadPoint(row.Date, row.Uploads))],
            [.. uploaderRows.Select(row => new TopUploader(row.DisplayName, row.PhotoCount, row.BytesStored))]);
    }
}
