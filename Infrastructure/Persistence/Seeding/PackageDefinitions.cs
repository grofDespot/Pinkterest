using Pinkterest.Domain.Entities;
using Pinkterest.Domain.Enums;

namespace Pinkterest.Infrastructure.Persistence.Seeding;

public static class PackageDefinitions
{
    private const long Megabyte = 1024L * 1024L;
    private const long Gigabyte = 1024L * Megabyte;

    public static IReadOnlyList<Package> All =>
    [
        new()
        {
            Tier = PackageTier.Free,
            Name = "FREE",
            MaxUploadSizeBytes = 2 * Megabyte,
            DailyUploadLimit = 5,
            MaxTotalStorageBytes = 50 * Megabyte,
            MonthlyPrice = 0m
        },
        new()
        {
            Tier = PackageTier.Pro,
            Name = "PRO",
            MaxUploadSizeBytes = 10 * Megabyte,
            DailyUploadLimit = 50,
            MaxTotalStorageBytes = 2 * Gigabyte,
            MonthlyPrice = 4.99m
        },
        new()
        {
            Tier = PackageTier.Gold,
            Name = "GOLD",
            MaxUploadSizeBytes = 50 * Megabyte,
            DailyUploadLimit = 500,
            MaxTotalStorageBytes = 20 * Gigabyte,
            MonthlyPrice = 14.99m
        }
    ];
}
