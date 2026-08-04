using System.ComponentModel.DataAnnotations;
using Pinkterest.Application.Photos.Processing;

namespace Pinkterest.Web.Models.Photos;

public sealed class UploadPhotoViewModel
{
    [Required(ErrorMessage = "Choose a photo to upload.")]
    [Display(Name = "Photo")]
    public IFormFile? File { get; set; }

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Hashtags")]
    [StringLength(500)]
    public string Hashtags { get; set; } = string.Empty;

    [Display(Name = "Save as")]
    public ImageOutputFormat Format { get; set; } = ImageOutputFormat.Original;

    [Range(0, 10000)]
    [Display(Name = "Maximum width")]
    public int? MaxWidth { get; set; }

    [Range(0, 10000)]
    [Display(Name = "Maximum height")]
    public int? MaxHeight { get; set; }

    [Display(Name = "Filters")]
    public List<string> SelectedFilters { get; set; } = [];

    public IReadOnlyList<string> AvailableFilters => ImageFilterCatalog.AvailableFilters;
}
