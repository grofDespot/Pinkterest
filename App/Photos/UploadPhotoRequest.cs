using Pinkterest.Application.Photos.Processing;

namespace Pinkterest.Application.Photos;

public sealed record UploadPhotoRequest(
    Guid OwnerId,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content,
    string Description,
    IReadOnlyList<string> Hashtags,
    ImageProcessingOptions Processing);
