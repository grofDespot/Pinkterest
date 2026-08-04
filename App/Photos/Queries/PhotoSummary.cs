namespace Pinkterest.Application.Photos.Queries;

public sealed record PhotoSummary(
    Guid Id,
    string Description,
    string AuthorDisplayName,
    Guid OwnerId,
    DateTimeOffset UploadedUtc,
    int Width,
    int Height,
    long SizeBytes,
    IReadOnlyList<string> Hashtags);
