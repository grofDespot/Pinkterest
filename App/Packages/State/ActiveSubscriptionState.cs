using Pinkterest.Application.Common.Results;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Packages.State;

public sealed class ActiveSubscriptionState : IPackageSubscriptionState
{
    public string Name => "Active";

    public Result<PackageChangePlan> RequestChange(ApplicationUser user, Package target, DateOnly today)
    {
        if (user.PackageId == target.Id)
        {
            return Result.Failure<PackageChangePlan>(PackageErrors.AlreadyOnPackage);
        }

        if (user.LastPackageChangeUtc is { } last &&
            DateOnly.FromDateTime(last.UtcDateTime) >= today)
        {
            return Result.Failure<PackageChangePlan>(PackageErrors.AlreadyChangedToday);
        }

        return Result.Success(new PackageChangePlan(target.Id, today.AddDays(1)));
    }
}
