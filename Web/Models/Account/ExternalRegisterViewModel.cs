using System.ComponentModel.DataAnnotations;
using Pinkterest.Application.Packages;

namespace Pinkterest.Web.Models.Account;

public sealed class ExternalRegisterViewModel
{
    public string Provider { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 2)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose a package.")]
    [Display(Name = "Package")]
    public Guid PackageId { get; set; }

    public IReadOnlyList<PackageDto> Packages { get; set; } = [];
}
