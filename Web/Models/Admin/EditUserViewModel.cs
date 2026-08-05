using System.ComponentModel.DataAnnotations;
using Pinkterest.Application.Admin.Models;
using Pinkterest.Application.Packages;

namespace Pinkterest.Web.Models.Admin;

public sealed class EditUserViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(128, MinimumLength = 2)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Package")]
    public Guid PackageId { get; set; }

    [Display(Name = "Clear lockout")]
    public bool ClearLockout { get; set; }

    public AdminUserDetail? Detail { get; set; }

    public IReadOnlyList<PackageDto> Packages { get; set; } = [];
}
