using System.ComponentModel.DataAnnotations;
using Pinkterest.Application.Packages;

namespace Pinkterest.Web.Models.Account;

public sealed class RegisterViewModel
{
    [Required]
    [StringLength(128, MinimumLength = 2)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(128, MinimumLength = 12, ErrorMessage = "The password must be at least 12 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose a package.")]
    [Display(Name = "Package")]
    public Guid PackageId { get; set; }

    public IReadOnlyList<PackageDto> Packages { get; set; } = [];
}
