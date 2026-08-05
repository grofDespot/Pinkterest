using System.ComponentModel.DataAnnotations;
using Pinkterest.Application.Packages;

namespace Pinkterest.Web.Models.Account;

public sealed class ChangePackageViewModel
{
    [Required(ErrorMessage = "Choose a package.")]
    [Display(Name = "New package")]
    public Guid TargetPackageId { get; set; }

    public Guid CurrentPackageId { get; set; }

    public string CurrentPackageName { get; set; } = string.Empty;

    public string? PendingPackageName { get; set; }

    public DateOnly? PendingEffectiveDate { get; set; }

    public IReadOnlyList<PackageDto> Packages { get; set; } = [];
}
