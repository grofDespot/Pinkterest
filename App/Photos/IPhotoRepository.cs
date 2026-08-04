using Pinkterest.Application.Common.Specifications;
using Pinkterest.Application.Photos.Queries;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Photos;

public interface IPhotoRepository
{
    Task<IReadOnlyList<PhotoSummary>> ListAsync(
        Specification<Photo> specification,
        int take,
        int skip = 0,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(Specification<Photo> specification, CancellationToken cancellationToken = default);

    Task<PhotoDetail?> GetDetailAsync(Guid photoId, CancellationToken cancellationToken = default);

    Task<Photo?> GetForUpdateAsync(Guid photoId, CancellationToken cancellationToken = default);

    Task<(string StorageKey, string ContentType, Guid OwnerId)?> GetStorageInfoAsync(
        Guid photoId,
        bool thumbnail,
        CancellationToken cancellationToken = default);
}
