using System.ComponentModel.DataAnnotations;
using Pinkterest.Application.Photos.Search;

namespace Pinkterest.Web.Models.Photos;

public sealed class PhotoSearchViewModel
{
    [Display(Name = "Hashtag")]
    [StringLength(64)]
    public string? Hashtag { get; set; }

    [Display(Name = "Author")]
    [StringLength(128)]
    public string? Author { get; set; }

    [Display(Name = "Minimum size (KB)")]
    [Range(0, 1_000_000)]
    public long? MinSizeKb { get; set; }

    [Display(Name = "Maximum size (KB)")]
    [Range(0, 1_000_000)]
    public long? MaxSizeKb { get; set; }

    [Display(Name = "Uploaded from")]
    [DataType(DataType.Date)]
    public DateOnly? UploadedFrom { get; set; }

    [Display(Name = "Uploaded to")]
    [DataType(DataType.Date)]
    public DateOnly? UploadedTo { get; set; }

    public int Page { get; set; } = 1;

    public PhotoSearchResult? Results { get; set; }

    public PhotoSearchQuery ToQuery() => new(
        Hashtag,
        Author,
        MinSizeKb * 1024,
        MaxSizeKb * 1024,
        UploadedFrom,
        UploadedTo,
        Page);
}
