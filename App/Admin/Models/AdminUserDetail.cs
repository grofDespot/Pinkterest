namespace Pinkterest.Application.Admin.Models;

public sealed record AdminUserDetail(
    Guid Id,
    string DisplayName,
    string Email,
    Guid PackageId,
    string PackageName,
    string? PendingPackageName,
    DateOnly? PendingPackageEffectiveDate,
    int PhotoCount,
    long BytesStored,
    DateTimeOffset CreatedUtc);
