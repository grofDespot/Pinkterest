using Pinkterest.Application.Common.Results;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Packages.State;

public sealed class ChangePendingState : IPackageSubscriptionState
{
    public string Name => "ChangePending";

    public Result<PackageChangePlan> RequestChange(ApplicationUser user, Package target, DateOnly today) =>
        Result.Failure<PackageChangePlan>(PackageErrors.ChangeAlreadyScheduled);
}
