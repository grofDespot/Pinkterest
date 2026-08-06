using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Pinkterest.Application.Photos.Storage;
using Pinkterest.Benchmarks.Support;
using Pinkterest.Infrastructure.Storage;

namespace Pinkterest.Benchmarks;

[MemoryDiagnoser]
public class StorageReadBenchmarks
{
    private const string ThumbnailKey = "thumbnails/benchmark/thumb.jpg";

    private string _root = string.Empty;
    private IPhotoStorage _direct = default!;
    private IPhotoStorage _cached = default!;

    [GlobalSetup]
    public async Task SetupAsync()
    {
        _root = Path.Combine(Path.GetTempPath(), "pinkterest-benchmarks", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);

        var options = Options.Create(new StorageOptions
        {
            Provider = StorageProvider.Local,
            LocalRootPath = _root
        });

        _direct = new LocalFileSystemPhotoStorage(options, new BenchmarkHostEnvironment());

        _cached = new CachingPhotoStorageProxy(
            new LocalFileSystemPhotoStorage(options, new BenchmarkHostEnvironment()),
            new MemoryCache(new MemoryCacheOptions { SizeLimit = 64L * 1024 * 1024 }));

        var payload = new byte[48 * 1024];
        Random.Shared.NextBytes(payload);

        using var source = new MemoryStream(payload);
        await _direct.SaveAsync(ThumbnailKey, source, "image/jpeg");

        await ReadAsync(_cached);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Benchmark(Baseline = true, Description = "One thumbnail, direct from disk")]
    public Task<long> DirectRead() => ReadAsync(_direct);

    [Benchmark(Description = "One thumbnail, through the caching proxy")]
    public Task<long> CachedRead() => ReadAsync(_cached);

    [Benchmark(Description = "Gallery page: ten thumbnails, direct")]
    public Task<long> TenThumbnailsDirect() => ReadManyAsync(_direct, 10);

    [Benchmark(Description = "Gallery page: ten thumbnails, cached")]
    public Task<long> TenThumbnailsCached() => ReadManyAsync(_cached, 10);

    private static async Task<long> ReadManyAsync(IPhotoStorage storage, int count)
    {
        var total = 0L;

        for (var i = 0; i < count; i++)
        {
            total += await ReadAsync(storage);
        }

        return total;
    }

    private static async Task<long> ReadAsync(IPhotoStorage storage)
    {
        await using var stream = await storage.OpenReadAsync(ThumbnailKey);
        using var sink = new MemoryStream();
        await stream.CopyToAsync(sink);

        return sink.Length;
    }
}
