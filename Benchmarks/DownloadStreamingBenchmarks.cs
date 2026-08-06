using BenchmarkDotNet.Attributes;

namespace Pinkterest.Benchmarks;

[MemoryDiagnoser]
public class DownloadStreamingBenchmarks
{
    private string _file = string.Empty;

    [Params(256 * 1024, 4 * 1024 * 1024)]
    public int FileSizeBytes { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _file = Path.Combine(Path.GetTempPath(), $"pinkterest-download-{Guid.NewGuid():N}.bin");

        var payload = new byte[FileSizeBytes];
        Random.Shared.NextBytes(payload);
        File.WriteAllBytes(_file, payload);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (File.Exists(_file))
        {
            File.Delete(_file);
        }
    }

    [Benchmark(Baseline = true, Description = "Buffer the whole file into memory first")]
    public async Task<long> BufferedIntoMemory()
    {
        await using var source = File.OpenRead(_file);
        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer);

        buffer.Position = 0;
        await buffer.CopyToAsync(Stream.Null);

        return buffer.Length;
    }

    [Benchmark(Description = "Stream straight to the response")]
    public async Task<long> StreamedToResponse()
    {
        await using var source = File.OpenRead(_file);
        await source.CopyToAsync(Stream.Null);

        return source.Length;
    }
}
