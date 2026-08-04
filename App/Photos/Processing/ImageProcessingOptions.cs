namespace Pinkterest.Application.Photos.Processing;

public sealed record ImageProcessingOptions(
    ImageOutputFormat Format,
    int? MaxWidth,
    int? MaxHeight,
    IReadOnlyList<string> Filters)
{
    public static ImageProcessingOptions None { get; } =
        new(ImageOutputFormat.Original, null, null, []);
}
