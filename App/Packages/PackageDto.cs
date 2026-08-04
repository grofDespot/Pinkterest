namespace Pinkterest.Application.Packages;

public sealed record PackageDto(
    Guid Id,
    string Name,
    long MaxUploadSizeBytes,
    int DailyUploadLimit,
    long MaxTotalStorageBytes,
    decimal MonthlyPrice);
