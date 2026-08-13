using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Photos.Processing;

namespace Pinkterest.Application.Photos.Presets;

public interface IFilterPresetService
{
    Task<IReadOnlyList<FilterPresetDto>> ListAsync(Guid ownerId, CancellationToken cancellationToken = default);

    Task<Result<ImageProcessingOptions>> GetOptionsAsync(
        Guid presetId,
        Guid ownerId,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> SaveAsync(
        Guid ownerId,
        string name,
        ImageProcessingOptions options,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid presetId, Guid ownerId, CancellationToken cancellationToken = default);
}
