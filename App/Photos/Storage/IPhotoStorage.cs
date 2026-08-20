namespace Pinkterest.Application.Photos.Storage;

public interface IPhotoStorage
{
    string ProviderName { get; }

    Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
