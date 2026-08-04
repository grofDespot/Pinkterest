using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Pinkterest.Application.Photos.Processing;

public sealed class ImageProcessor : IImageProcessor
{
    public async Task<ProcessedImage> ProcessAsync(
        Stream source,
        ImageProcessingOptions options,
        CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(source, cancellationToken);

        ImagePipeline.Compose(BuildFilters(options)).Apply(image);

        var output = new MemoryStream();
        await image.SaveAsync(output, ResolveEncoder(options.Format, image), cancellationToken);
        output.Position = 0;

        return new ProcessedImage(
            output,
            ResolveContentType(options.Format, image),
            ResolveExtension(options.Format, image),
            image.Width,
            image.Height,
            output.Length);
    }

    private static IReadOnlyList<IImageFilter> BuildFilters(ImageProcessingOptions options)
    {
        var filters = new List<IImageFilter>();

        if (options.MaxWidth is > 0 || options.MaxHeight is > 0)
        {
            filters.Add(new ResizeFilter(options.MaxWidth ?? 0, options.MaxHeight ?? 0));
        }

        filters.AddRange(ImageFilterCatalog.CreateMany(options.Filters));
        return filters;
    }

    private static IImageEncoder ResolveEncoder(ImageOutputFormat format, Image image) => format switch
    {
        ImageOutputFormat.Jpeg => new JpegEncoder { Quality = 90 },
        ImageOutputFormat.Png => new PngEncoder(),
        ImageOutputFormat.Bmp => new BmpEncoder(),
        _ => image.Metadata.DecodedImageFormat is { } decoded
            ? SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager.GetEncoder(decoded)
            : new JpegEncoder { Quality = 90 }
    };

    private static string ResolveContentType(ImageOutputFormat format, Image image) => format switch
    {
        ImageOutputFormat.Jpeg => "image/jpeg",
        ImageOutputFormat.Png => "image/png",
        ImageOutputFormat.Bmp => "image/bmp",
        _ => image.Metadata.DecodedImageFormat?.DefaultMimeType ?? "image/jpeg"
    };

    private static string ResolveExtension(ImageOutputFormat format, Image image) => format switch
    {
        ImageOutputFormat.Jpeg => ".jpg",
        ImageOutputFormat.Png => ".png",
        ImageOutputFormat.Bmp => ".bmp",
        _ => image.Metadata.DecodedImageFormat?.FileExtensions.FirstOrDefault() is { } extension
            ? $".{extension}"
            : ".jpg"
    };
}
