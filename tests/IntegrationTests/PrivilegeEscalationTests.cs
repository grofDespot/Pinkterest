using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pinkterest.Domain.Enums;
using Pinkterest.IntegrationTests.Infrastructure;
using Xunit;

namespace Pinkterest.IntegrationTests;

[Collection(PinkterestCollection.Name)]
public class PrivilegeEscalationTests(PinkterestFixture fixture)
{
    private async Task<HttpClient> RegisterAndSignInAsync()
    {
        var client = fixture.CreateClient();
        var email = $"user-{Guid.CreateVersion7():N}@pinkterest.test";

        var freePackageId = await fixture.UseDbContextAsync(context =>
            context.Packages.Where(p => p.Tier == PackageTier.Free).Select(p => p.Id).SingleAsync());

        await client.PostFormAsync("/Account/Register", new Dictionary<string, string>
        {
            ["DisplayName"] = "Regular user",
            ["Email"] = email,
            ["Password"] = TestCredentials.UserPassword,
            ["ConfirmPassword"] = TestCredentials.UserPassword,
            ["PackageId"] = freePackageId.ToString()
        });

        return client;
    }

    [Theory]
    [InlineData("/Admin")]
    [InlineData("/Admin/Users")]
    [InlineData("/Admin/AuditLog")]
    [InlineData("/Admin/Photos")]
    public async Task A_registered_user_cannot_reach_the_admin_area(string path)
    {
        var client = await RegisterAndSignInAsync();

        var response = await client.GetAsync(path);

        response.StatusCode.Should().NotBe(HttpStatusCode.OK,
            "{0} is restricted to the Administrator policy", path);
        response.RedirectsTo("/Account/AccessDenied").Should().BeTrue();
    }

    [Fact]
    public async Task A_registered_user_reaches_their_own_usage_page()
    {
        var client = await RegisterAndSignInAsync();

        var response = await client.GetAsync("/Account/Usage");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Registration_cannot_grant_the_administrator_role()
    {
        var client = fixture.CreateClient();
        var email = $"climber-{Guid.CreateVersion7():N}@pinkterest.test";

        var freePackageId = await fixture.UseDbContextAsync(context =>
            context.Packages.Where(p => p.Tier == PackageTier.Free).Select(p => p.Id).SingleAsync());

        await client.PostFormAsync("/Account/Register", new Dictionary<string, string>
        {
            ["DisplayName"] = "Climber",
            ["Email"] = email,
            ["Password"] = TestCredentials.UserPassword,
            ["ConfirmPassword"] = TestCredentials.UserPassword,
            ["PackageId"] = freePackageId.ToString(),
            ["Roles"] = "Administrator",
            ["IsAdministrator"] = "true"
        });

        var response = await client.GetAsync("/Admin");

        response.StatusCode.Should().NotBe(HttpStatusCode.OK,
            "extra form fields must not be bound into role assignment");
    }
}
