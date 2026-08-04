namespace Pinkterest.Application.Packages;

public interface IPackageCatalog
{
    Task<IReadOnlyList<PackageDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
