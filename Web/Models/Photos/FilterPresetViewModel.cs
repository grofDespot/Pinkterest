using System.ComponentModel.DataAnnotations;
using Pinkterest.Application.Photos.Presets;
using Pinkterest.Application.Photos.Processing;

namespace Pinkterest.Web.Models.Photos;

public sealed class SavePresetViewModel
{
    [Required(ErrorMessage = "Give the preset a name.")]
    [StringLength(64, MinimumLength = 1)]
    [Display(Name = "Preset name")]
    public string Name { get; set; } = string.Empty;

    public Guid PhotoId { get; set; }

    public ImageOutputFormat Format { get; set; } = ImageOutputFormat.Original;

    public int? MaxWidth { get; set; }

    public int? MaxHeight { get; set; }

    public List<string> Filters { get; set; } = [];

    public ImageProcessingOptions ToProcessingOptions() => new(Format, MaxWidth, MaxHeight, Filters);
}

public sealed class PresetListViewModel
{
    public IReadOnlyList<FilterPresetDto> Presets { get; set; } = [];
}
