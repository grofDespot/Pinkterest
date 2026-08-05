using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Packages.State;

namespace Pinkterest.Application.Packages;

public interface IPackageChangeService
{
    Task<Result<PackageChangePlan>> RequestChangeAsync(
        Guid userId,
        Guid targetPackageId,
        CancellationToken cancellationToken = default);

    Task<int> ApplyDueChangesAsync(CancellationToken cancellationToken = default);
}
