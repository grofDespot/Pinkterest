namespace Pinkterest.Application.Usage;

public sealed record UsageSummary(
    string PackageName,
    long MaxUploadSizeBytes,
    int DailyUploadLimit,
    long MaxTotalStorageBytes,
    int UploadsToday,
    long BytesUploadedToday,
    long TotalBytesStored,
    string? PendingPackageName,
    DateOnly? PendingPackageEffectiveDate)
{
    public int RemainingUploadsToday => Math.Max(0, DailyUploadLimit - UploadsToday);

    public long RemainingStorageBytes => Math.Max(0, MaxTotalStorageBytes - TotalBytesStored);

    public double DailyUploadPercentage =>
        DailyUploadLimit <= 0 ? 0 : Math.Min(100d, UploadsToday * 100d / DailyUploadLimit);

    public double StoragePercentage =>
        MaxTotalStorageBytes <= 0 ? 0 : Math.Min(100d, TotalBytesStored * 100d / MaxTotalStorageBytes);
}
