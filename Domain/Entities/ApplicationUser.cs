using Microsoft.AspNetCore.Identity;

namespace Pinkterest.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public Guid PackageId { get; set; }

    public Package Package { get; set; } = null!;

    public Guid? PendingPackageId { get; set; }

    public Package? PendingPackage { get; set; }

    public DateOnly? PendingPackageEffectiveDate { get; set; }

    public DateTimeOffset? LastPackageChangeUtc { get; set; }

    public DateTimeOffset CreatedUtc { get; set; }

    public ICollection<Photo> Photos { get; set; } = [];

    public ICollection<FilterPreset> FilterPresets { get; set; } = [];
}
