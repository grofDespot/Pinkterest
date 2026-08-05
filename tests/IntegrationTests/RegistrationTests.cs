using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.Domain.Enums;
using Pinkterest.IntegrationTests.Infrastructure;
using Xunit;

namespace Pinkterest.IntegrationTests;

[Collection(PinkterestCollection.Name)]
public class RegistrationTests(PinkterestFixture fixture)
{
    [Fact]
    public async Task Registering_creates_the_user_on_the_chosen_package()
    {
        var email = $"anna-{Guid.CreateVersion7():N}@pinkterest.test";
        var client = fixture.CreateClient();

        var proPackageId = await fixture.UseDbContextAsync(context =>
            context.Packages.Where(p => p.Tier == PackageTier.Pro).Select(p => p.Id).SingleAsync());

        var response = await client.PostFormAsync("/Account/Register", new Dictionary<string, string>
        {
            ["DisplayName"] = "Anna",
            ["Email"] = email,
            ["Password"] = TestCredentials.UserPassword,
            ["ConfirmPassword"] = TestCredentials.UserPassword,
            ["PackageId"] = proPackageId.ToString()
        });

        response.RedirectsTo("/Account/Usage").Should().BeTrue();

        var user = await fixture.UseDbContextAsync(context =>
            context.Users.Include(u => u.Package).SingleOrDefaultAsync(u => u.Email == email));

        user.Should().NotBeNull();
        user!.DisplayName.Should().Be("Anna");
        user.Package.Tier.Should().Be(PackageTier.Pro);
    }

    [Fact]
    public async Task Registering_writes_an_audit_entry()
    {
        var email = $"boris-{Guid.CreateVersion7():N}@pinkterest.test";
        var client = fixture.CreateClient();

        var freePackageId = await fixture.UseDbContextAsync(context =>
            context.Packages.Where(p => p.Tier == PackageTier.Free).Select(p => p.Id).SingleAsync());

        await client.PostFormAsync("/Account/Register", new Dictionary<string, string>
        {
            ["DisplayName"] = "Boris",
            ["Email"] = email,
            ["Password"] = TestCredentials.UserPassword,
            ["ConfirmPassword"] = TestCredentials.UserPassword,
            ["PackageId"] = freePackageId.ToString()
        });

        var entry = await fixture.UseDbContextAsync(context => context.AuditLog
            .Where(a => a.Action == AuditActions.Register && a.UserName == email)
            .OrderByDescending(a => a.OccurredUtc)
            .FirstOrDefaultAsync());

        entry.Should().NotBeNull("every action must be logged");
        entry!.Succeeded.Should().BeTrue();
        entry.EntityType.Should().Be("ApplicationUser");
    }

    [Fact]
    public async Task A_form_posted_without_an_antiforgery_token_is_rejected()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["DisplayName"] = "Mallory",
                ["Email"] = "mallory@pinkterest.test",
                ["Password"] = TestCredentials.UserPassword,
                ["ConfirmPassword"] = TestCredentials.UserPassword
            }));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest,
            "AutoValidateAntiforgeryToken is registered globally");
    }
}
