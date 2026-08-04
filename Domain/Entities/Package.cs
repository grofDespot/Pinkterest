using Pinkterest.Domain.Common;
using Pinkterest.Domain.Enums;

namespace Pinkterest.Domain.Entities;

public class Package : Entity
{
    public PackageTier Tier { get; set; }

    public string Name { get; set; } = string.Empty;

    public long MaxUploadSizeBytes { get; set; }

    public int DailyUploadLimit { get; set; }

    public long MaxTotalStorageBytes { get; set; }

    public decimal MonthlyPrice { get; set; }

    public ICollection<ApplicationUser> Subscribers { get; set; } = [];
}
