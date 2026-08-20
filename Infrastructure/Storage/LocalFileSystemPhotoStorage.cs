using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pinkterest.Application.Photos.Storage;

namespace Pinkterest.Infrastructure.Storage;

public sealed class LocalFileSystemPhotoStorage : IPhotoStorage
{
    private readonly string _root;

    public LocalFileSystemPhotoStorage(IOptions<StorageOptions> options, IHostEnvironment environment)
    {
        var configured = options.Value.LocalRootPath;

        _root = Path.GetFullPath(Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured));

        Directory.CreateDirectory(_root);
    }

    public string ProviderName => "local";

    public async Task SaveAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var target = new FileStream(
            path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);

        await content.CopyToAsync(target, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The requested object does not exist in local storage.", key);
        }

        Stream stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);

        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(File.Exists(ResolvePath(key)));

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A storage key is required.", nameof(key));
        }

        var normalized = key.Replace('\\', '/').TrimStart('/');
        var candidate = Path.GetFullPath(Path.Combine(_root, normalized));

        if (!candidate.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The storage key resolves outside the storage root.");
        }

        return candidate;
    }
}
