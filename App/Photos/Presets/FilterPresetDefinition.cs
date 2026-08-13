using Pinkterest.Application.Photos.Processing;

namespace Pinkterest.Application.Photos.Presets;

public sealed record FilterPresetDefinition(
    ImageOutputFormat Format,
    int? MaxWidth,
    int? MaxHeight,
    IReadOnlyList<string> Filters)
{
    public ImageProcessingOptions ToOptions() => new(Format, MaxWidth, MaxHeight, Filters);

    public static FilterPresetDefinition From(ImageProcessingOptions options) =>
        new(options.Format, options.MaxWidth, options.MaxHeight, options.Filters);

    public FilterPresetDefinition WithKnownFiltersOnly() =>
        this with { Filters = ImageFilterCatalog.CreateMany(Filters).Select(filter => filter.Name).ToList() };
}
