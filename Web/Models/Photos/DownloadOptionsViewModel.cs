using Pinkterest.Application.Photos.Processing;

namespace Pinkterest.Web.Models.Photos;

public sealed class DownloadOptionsViewModel
{
    public ImageOutputFormat Format { get; set; } = ImageOutputFormat.Original;

    public int? MaxWidth { get; set; }

    public int? MaxHeight { get; set; }

    public List<string> Filters { get; set; } = [];

    public ImageProcessingOptions ToProcessingOptions() =>
        new(Format, MaxWidth, MaxHeight, Filters);
}
