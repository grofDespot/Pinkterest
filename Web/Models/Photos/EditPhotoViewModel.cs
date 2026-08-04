using System.ComponentModel.DataAnnotations;

namespace Pinkterest.Web.Models.Photos;

public sealed class EditPhotoViewModel
{
    public Guid Id { get; set; }

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Hashtags")]
    [StringLength(500)]
    public string Hashtags { get; set; } = string.Empty;
}
