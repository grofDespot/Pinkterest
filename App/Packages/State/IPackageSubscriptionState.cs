using Pinkterest.Application.Common.Results;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Packages.State;

public interface IPackageSubscriptionState
{
    string Name { get; }

    Result<PackageChangePlan> RequestChange(ApplicationUser user, Package target, DateOnly today);
}
