using Microsoft.Extensions.Caching.Memory;
using Pinkterest.Application.Photos.Storage;

namespace Pinkterest.Infrastructure.Storage;

public sealed class CachingPhotoStorageProxy(IPhotoStorage inner, IMemoryCache cache) : IPhotoStorage
{
    private const string CachePrefix = "photo-storage:";
    private const long MaxCacheableBytes = 512 * 1024;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public string ProviderName => $"cached:{inner.ProviderName}";

    public async Task SaveAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await inner.SaveAsync(key, content, contentType, cancellationToken);
        cache.Remove(CachePrefix + key);
    }

    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!IsCacheable(key))
        {
            return await inner.OpenReadAsync(key, cancellationToken);
        }

        if (cache.TryGetValue(CachePrefix + key, out byte[]? cached) && cached is not null)
        {
            return new MemoryStream(cached, writable: false);
        }

        await using var source = await inner.OpenReadAsync(key, cancellationToken);
        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, cancellationToken);

        var bytes = buffer.ToArray();

        if (bytes.LongLength <= MaxCacheableBytes)
        {
            cache.Set(CachePrefix + key, bytes, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                Size = bytes.LongLength
            });
        }

        return new MemoryStream(bytes, writable: false);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        inner.ExistsAsync(key, cancellationToken);

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await inner.DeleteAsync(key, cancellationToken);
        cache.Remove(CachePrefix + key);
    }

    private static bool IsCacheable(string key) => key.StartsWith("thumbnails/", StringComparison.Ordinal);
}
