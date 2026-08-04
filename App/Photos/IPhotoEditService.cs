using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos;

public interface IPhotoEditService
{
    Task<Result> UpdateDetailsAsync(
        Guid photoId,
        Guid editorId,
        bool editorIsAdministrator,
        string description,
        IReadOnlyList<string> hashtags,
        CancellationToken cancellationToken = default);
}
