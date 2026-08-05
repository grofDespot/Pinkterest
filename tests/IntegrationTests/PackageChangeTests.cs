using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Packages;
using Pinkterest.Domain.Enums;
using Pinkterest.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Pinkterest.IntegrationTests;

[Collection(PinkterestCollection.Name)]
public class PackageChangeTests(PinkterestFixture fixture)
{
    [Fact]
    public async Task A_change_is_scheduled_for_tomorrow_and_a_second_one_is_refused()
    {
        var email = $"switcher-{Guid.CreateVersion7():N}@pinkterest.test";
        var client = fixture.CreateClient();

        var packages = await fixture.UseDbContextAsync(context =>
            context.Packages.ToDictionaryAsync(p => p.Tier, p => p.Id));

        await client.PostFormAsync("/Account/Register", new Dictionary<string, string>
        {
            ["DisplayName"] = "Switcher",
            ["Email"] = email,
            ["Password"] = TestCredentials.UserPassword,
            ["ConfirmPassword"] = TestCredentials.UserPassword,
            ["PackageId"] = packages[PackageTier.Free].ToString()
        });

        var userId = await fixture.UseDbContextAsync(context =>
            context.Users.Where(u => u.Email == email).Select(u => u.Id).SingleAsync());

        using var scope = fixture.Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPackageChangeService>();

        var first = await service.RequestChangeAsync(userId, packages[PackageTier.Pro]);
        first.IsSuccess.Should().BeTrue();
        first.Value.EffectiveDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1));

        var second = await service.RequestChangeAsync(userId, packages[PackageTier.Gold]);
        second.IsFailure.Should().BeTrue();
        second.Error.Code.Should().Be("Package.ChangeScheduled");

        var user = await fixture.UseDbContextAsync(context =>
            context.Users.AsNoTracking().SingleAsync(u => u.Id == userId));

        user.PackageId.Should().Be(packages[PackageTier.Free], "the change only takes effect tomorrow");
        user.PendingPackageId.Should().Be(packages[PackageTier.Pro]);
    }
}
