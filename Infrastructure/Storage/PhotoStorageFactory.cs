using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pinkterest.Application.Photos.Storage;

namespace Pinkterest.Infrastructure.Storage;

public sealed class PhotoStorageFactory(
    IOptions<StorageOptions> options,
    IServiceProvider serviceProvider) : IPhotoStorageFactory
{
    public IPhotoStorage Create() => options.Value.Provider switch
    {
        StorageProvider.Local => serviceProvider.GetRequiredService<LocalFileSystemPhotoStorage>(),
        StorageProvider.S3 => serviceProvider.GetRequiredService<S3PhotoStorage>(),
        _ => throw new InvalidOperationException($"Unknown storage provider '{options.Value.Provider}'.")
    };
}
