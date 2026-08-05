namespace Pinkterest.Application.Admin.Models;

public sealed record AdminStatistics(
    int UserCount,
    int PhotoCount,
    long TotalBytesStored,
    int UploadsToday,
    IReadOnlyList<PackageDistribution> PackageDistribution,
    IReadOnlyList<DailyUploadPoint> RecentUploads,
    IReadOnlyList<TopUploader> TopUploaders);

public sealed record PackageDistribution(string PackageName, int UserCount);

public sealed record DailyUploadPoint(DateOnly Date, int Uploads);

public sealed record TopUploader(string DisplayName, int PhotoCount, long BytesStored);
