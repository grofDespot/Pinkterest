namespace Pinkterest.Application.Admin.Models;

public sealed record AdminUserSummary(
    Guid Id,
    string DisplayName,
    string Email,
    string PackageName,
    int PhotoCount,
    long BytesStored,
    DateTimeOffset CreatedUtc,
    bool IsLockedOut);
