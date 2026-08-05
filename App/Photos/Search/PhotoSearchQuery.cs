namespace Pinkterest.Application.Photos.Search;

public sealed record PhotoSearchQuery(
    string? Hashtag = null,
    string? Author = null,
    long? MinSizeBytes = null,
    long? MaxSizeBytes = null,
    DateOnly? UploadedFrom = null,
    DateOnly? UploadedTo = null,
    int Page = 1,
    int PageSize = 12);
