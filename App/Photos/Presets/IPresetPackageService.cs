using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Presets;

public interface IPresetPackageService
{
    Task<Result<byte[]>> ExportAsync(Guid presetId, Guid ownerId, CancellationToken cancellationToken = default);

    Task<Result<Guid>> ImportAsync(Guid ownerId, byte[] package, CancellationToken cancellationToken = default);
}
