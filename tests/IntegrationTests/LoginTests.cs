using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.IntegrationTests.Infrastructure;
using Xunit;

namespace Pinkterest.IntegrationTests;

[Collection(PinkterestCollection.Name)]
public class LoginTests(PinkterestFixture fixture)
{
    private static async Task<string> BodyOfFailedLoginAsync(
        PinkterestFixture fixture, string email, string password)
    {
        var client = fixture.CreateClient();

        var response = await client.PostFormAsync("/Account/Login", new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password
        });

        return await response.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task An_unknown_email_and_a_wrong_password_produce_the_same_message()
    {
        var unknownUser = await BodyOfFailedLoginAsync(
            fixture, "nobody@pinkterest.test", TestCredentials.UserPassword);

        var wrongPassword = await BodyOfFailedLoginAsync(
            fixture, TestCredentials.AdministratorEmail, "Wrong!Password#2026");

        const string message = "The email address or password is incorrect.";

        unknownUser.Should().Contain(message);
        wrongPassword.Should().Contain(message,
            "differing messages would let an attacker enumerate registered accounts");
    }

    [Fact]
    public async Task A_failed_login_is_audited_as_unsuccessful()
    {
        var email = $"ghost-{Guid.CreateVersion7():N}@pinkterest.test";

        await BodyOfFailedLoginAsync(fixture, email, "Wrong!Password#2026");

        var entry = await fixture.UseDbContextAsync(context => context.AuditLog
            .Where(a => a.Action == AuditActions.LoginFailed && a.UserName == email)
            .FirstOrDefaultAsync());

        entry.Should().NotBeNull();
        entry!.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task The_administrator_can_sign_in_and_reach_the_admin_area()
    {
        var client = fixture.CreateClient();

        var login = await client.PostFormAsync("/Account/Login", new Dictionary<string, string>
        {
            ["Email"] = TestCredentials.AdministratorEmail,
            ["Password"] = TestCredentials.AdministratorPassword
        });

        login.RedirectsTo("/").Should().BeTrue();

        var admin = await client.GetAsync("/Admin");
        admin.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
