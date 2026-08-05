using FluentAssertions;
using Pinkterest.Application.Packages.State;
using Pinkterest.Domain.Entities;
using Pinkterest.Domain.Enums;
using Xunit;

namespace Pinkterest.UnitTests.Packages;

public class PackageSubscriptionStateTests
{
    private static readonly DateOnly Today = new(2026, 8, 5);

    private static Package Package(PackageTier tier) => new() { Tier = tier, Name = tier.ToString() };

    private static ApplicationUser UserOn(Package package) => new()
    {
        DisplayName = "Anna",
        PackageId = package.Id,
        Package = package
    };

    [Fact]
    public void An_active_subscription_schedules_the_change_for_tomorrow()
    {
        var current = Package(PackageTier.Free);
        var target = Package(PackageTier.Pro);

        var result = PackageSubscriptionStateFactory.For(UserOn(current), Today)
            .RequestChange(UserOn(current), target, Today);

        result.IsSuccess.Should().BeTrue();
        result.Value.TargetPackageId.Should().Be(target.Id);
        result.Value.EffectiveDate.Should().Be(Today.AddDays(1));
    }

    [Fact]
    public void Switching_to_the_package_you_are_already_on_is_rejected()
    {
        var current = Package(PackageTier.Free);
        var user = UserOn(current);

        var result = new ActiveSubscriptionState().RequestChange(user, current, Today);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Package.AlreadyActive");
    }

    [Fact]
    public void A_second_change_on_the_same_day_is_rejected()
    {
        var user = UserOn(Package(PackageTier.Free));
        user.LastPackageChangeUtc = new DateTimeOffset(Today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var result = new ActiveSubscriptionState().RequestChange(user, Package(PackageTier.Gold), Today);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Package.AlreadyChangedToday");
    }

    [Fact]
    public void A_change_made_yesterday_does_not_block_today()
    {
        var user = UserOn(Package(PackageTier.Free));
        user.LastPackageChangeUtc =
            new DateTimeOffset(Today.AddDays(-1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var result = new ActiveSubscriptionState().RequestChange(user, Package(PackageTier.Gold), Today);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void A_user_with_a_pending_change_is_in_the_pending_state()
    {
        var user = UserOn(Package(PackageTier.Free));
        user.PendingPackageId = Guid.CreateVersion7();
        user.PendingPackageEffectiveDate = Today.AddDays(1);

        PackageSubscriptionStateFactory.For(user, Today).Should().BeOfType<ChangePendingState>();
    }

    [Fact]
    public void The_pending_state_refuses_a_further_change()
    {
        var user = UserOn(Package(PackageTier.Free));

        var result = new ChangePendingState().RequestChange(user, Package(PackageTier.Gold), Today);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Package.ChangeScheduled");
    }

    [Fact]
    public void A_pending_change_whose_date_has_arrived_no_longer_blocks()
    {
        var user = UserOn(Package(PackageTier.Free));
        user.PendingPackageId = Guid.CreateVersion7();
        user.PendingPackageEffectiveDate = Today;

        PackageSubscriptionStateFactory.For(user, Today).Should().BeOfType<ActiveSubscriptionState>();
    }
}
