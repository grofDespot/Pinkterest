using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Photos.Processing;

namespace Pinkterest.Application.Photos.Download;

public interface IPhotoDownloadService
{
    Task<Result<PhotoDownload>> PrepareAsync(
        Guid photoId,
        ImageProcessingOptions options,
        CancellationToken cancellationToken = default);
}
