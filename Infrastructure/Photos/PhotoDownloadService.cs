using Microsoft.Extensions.Logging;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Photos;
using Pinkterest.Application.Photos.Download;
using Pinkterest.Application.Photos.Processing;
using Pinkterest.Application.Photos.Storage;

using Pinkterest.Application.Common.Auditing;

using Pinkterest.CrossCutting.Auditing;

using Pinkterest.CrossCutting.Metrics;

namespace Pinkterest.Infrastructure.Photos;

public sealed class PhotoDownloadService(
    IPhotoRepository repository,
    IPhotoStorage storage,
    IImageProcessor imageProcessor,
    ILogger<PhotoDownloadService> logger) : IPhotoDownloadService
{
    [Audited(AuditActions.PhotoDownload, EntityType = "Photo")]
    [Measured(AuditActions.PhotoDownload)]
    public async Task<Result<PhotoDownload>> PrepareAsync(
        Guid photoId,
        ImageProcessingOptions options,
        CancellationToken cancellationToken = default)
    {
        var info = await repository.GetStorageInfoAsync(photoId, thumbnail: false, cancellationToken);

        if (info is not { } stored)
        {
            return Result.Failure<PhotoDownload>(Error.NotFound("Photo"));
        }

        if (!await storage.ExistsAsync(stored.StorageKey, cancellationToken))
        {
            logger.LogWarning("Photo {PhotoId} is missing from storage at {Key}.", photoId, stored.StorageKey);
            return Result.Failure<PhotoDownload>(Error.NotFound("Photo"));
        }

        var source = await storage.OpenReadAsync(stored.StorageKey, cancellationToken);

        if (IsUnchanged(options))
        {
            return Result.Success(new PhotoDownload(
                source,
                stored.ContentType,
                $"pinkterest-{photoId:N}{Path.GetExtension(stored.StorageKey)}"));
        }

        await using (source)
        {
            var processed = await imageProcessor.ProcessAsync(source, options, cancellationToken);

            return Result.Success(new PhotoDownload(
                processed.Content,
                processed.ContentType,
                $"pinkterest-{photoId:N}{processed.FileExtension}"));
        }
    }

    private static bool IsUnchanged(ImageProcessingOptions options) =>
        options.Format == ImageOutputFormat.Original
        && options.MaxWidth is null or 0
        && options.MaxHeight is null or 0
        && options.Filters.Count == 0;
}
