namespace Pinkterest.Application.Photos.Queries;

public sealed record PhotoDetail(
    Guid Id,
    string Description,
    string OriginalFileName,
    string ContentType,
    string AuthorDisplayName,
    Guid OwnerId,
    DateTimeOffset UploadedUtc,
    int Width,
    int Height,
    long SizeBytes,
    IReadOnlyList<string> Hashtags);
