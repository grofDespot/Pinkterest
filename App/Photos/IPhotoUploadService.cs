using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos;

public interface IPhotoUploadService
{
    Task<Result<Guid>> UploadAsync(UploadPhotoRequest request, CancellationToken cancellationToken = default);
}
