using BenchmarkDotNet.Attributes;
using Pinkterest.Application.Photos.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace Pinkterest.Benchmarks;

[MemoryDiagnoser]
public class ImageProcessingBenchmarks
{
    private readonly IImageProcessor _processor = new ImageProcessor();
    private byte[] _original = [];

    [GlobalSetup]
    public void Setup()
    {
        using var image = new Image<Rgba32>(1920, 1080);

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (var x = 0; x < row.Length; x++)
                {
                    row[x] = new Rgba32((byte)(x % 256), (byte)(y % 256), (byte)((x + y) % 256));
                }
            }
        });

        using var buffer = new MemoryStream();
        image.Save(buffer, new JpegEncoder { Quality = 90 });
        _original = buffer.ToArray();
    }

    [Benchmark(Baseline = true, Description = "Unchanged download: copy stored bytes")]
    public async Task<long> PassThrough()
    {
        await using var source = new MemoryStream(_original, writable: false);
        await source.CopyToAsync(Stream.Null);

        return source.Length;
    }

    [Benchmark(Description = "Decode and re-encode, no filters")]
    public Task<long> ReEncodeOnly() =>
        ProcessAsync(new ImageProcessingOptions(ImageOutputFormat.Jpeg, null, null, []));

    [Benchmark(Description = "Resize to 800px")]
    public Task<long> Resize() =>
        ProcessAsync(new ImageProcessingOptions(ImageOutputFormat.Jpeg, 800, 800, []));

    [Benchmark(Description = "Resize plus sepia plus blur")]
    public Task<long> ResizeSepiaBlur() =>
        ProcessAsync(new ImageProcessingOptions(ImageOutputFormat.Jpeg, 800, 800, ["sepia", "blur"]));

    [Benchmark(Description = "Thumbnail at 400px")]
    public Task<long> Thumbnail() =>
        ProcessAsync(new ImageProcessingOptions(ImageOutputFormat.Jpeg, 400, 400, []));

    private async Task<long> ProcessAsync(ImageProcessingOptions options)
    {
        await using var source = new MemoryStream(_original, writable: false);
        await using var processed = await _processor.ProcessAsync(source, options);

        return processed.SizeBytes;
    }
}
