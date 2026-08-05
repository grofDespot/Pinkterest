using FluentAssertions;
using Microsoft.Playwright;
using Pinkterest.UiTests.Infrastructure;
using Xunit;

namespace Pinkterest.UiTests;

[Collection(BrowserCollection.Name)]
public class RegistrationJourneyTests(BrowserFixture fixture)
{
    [Fact]
    public async Task A_new_user_can_register_on_the_free_package_and_see_their_quota()
    {
        var page = await fixture.NewPageAsync();
        var email = TestUser.NewEmail();

        await page.GotoAsync("/Account/Register");
        await page.FillAsync("#DisplayName", "UI test user");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", TestUser.Password);
        await page.FillAsync("#ConfirmPassword", TestUser.Password);
        await page.GetByText("FREE", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Create account" }).ClickAsync();

        page.Url.Should().Contain("/Account/Usage");

        await Assertions.Expect(page.GetByText("FREE")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Your usage" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task A_signed_in_user_sees_upload_and_usage_in_the_navigation()
    {
        var page = await fixture.NewPageAsync();
        var email = TestUser.NewEmail();

        await page.GotoAsync("/Account/Register");
        await page.FillAsync("#DisplayName", "Nav test user");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", TestUser.Password);
        await page.FillAsync("#ConfirmPassword", TestUser.Password);
        await page.GetByText("FREE", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Create account" }).ClickAsync();

        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Upload" })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Usage" })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Admin" })).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task Password_confirmation_must_match()
    {
        var page = await fixture.NewPageAsync();

        await page.GotoAsync("/Account/Register");
        await page.FillAsync("#DisplayName", "Mismatch");
        await page.FillAsync("#Email", TestUser.NewEmail());
        await page.FillAsync("#Password", TestUser.Password);
        await page.FillAsync("#ConfirmPassword", "Something!Different#2026");
        await page.GetByRole(AriaRole.Button, new() { Name = "Create account" }).ClickAsync();

        await Assertions.Expect(page.GetByText("The passwords do not match.")).ToBeVisibleAsync();
        page.Url.Should().Contain("/Account/Register");
    }
}
