using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Packages.State;

public static class PackageSubscriptionStateFactory
{
    public static IPackageSubscriptionState For(ApplicationUser user, DateOnly today) =>
        user.PendingPackageId is not null && user.PendingPackageEffectiveDate > today
            ? new ChangePendingState()
            : new ActiveSubscriptionState();
}
