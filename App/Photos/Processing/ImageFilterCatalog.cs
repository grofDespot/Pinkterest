namespace Pinkterest.Application.Photos.Processing;

public static class ImageFilterCatalog
{
    public static IReadOnlyList<string> AvailableFilters { get; } = ["sepia", "grayscale", "blur"];

    public static IImageFilter? Create(string name) => name.Trim().ToLowerInvariant() switch
    {
        "sepia" => new SepiaFilter(),
        "grayscale" => new GrayscaleFilter(),
        "blur" => new BlurFilter(3f),
        _ => null
    };

    public static IReadOnlyList<IImageFilter> CreateMany(IEnumerable<string> names) =>
        names.Select(Create).OfType<IImageFilter>().ToList();
}
