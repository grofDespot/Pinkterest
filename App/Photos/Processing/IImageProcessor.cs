namespace Pinkterest.Application.Photos.Processing;

public interface IImageProcessor
{
    Task<ProcessedImage> ProcessAsync(
        Stream source,
        ImageProcessingOptions options,
        CancellationToken cancellationToken = default);
}
