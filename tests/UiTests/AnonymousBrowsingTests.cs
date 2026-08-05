using FluentAssertions;
using Microsoft.Playwright;
using Pinkterest.UiTests.Infrastructure;
using Xunit;

namespace Pinkterest.UiTests;

[Collection(BrowserCollection.Name)]
public class AnonymousBrowsingTests(BrowserFixture fixture)
{
    [Fact]
    public async Task The_gallery_is_visible_without_signing_in()
    {
        var page = await fixture.NewPageAsync();

        await page.GotoAsync("/Gallery");

        (await page.TitleAsync()).Should().Contain("Gallery");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Latest photos" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task An_anonymous_visitor_is_offered_sign_in_rather_than_upload()
    {
        var page = await fixture.NewPageAsync();

        await page.GotoAsync("/");

        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Sign in" }))
            .ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Upload" }))
            .Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task Visiting_upload_while_signed_out_lands_on_the_login_page()
    {
        var page = await fixture.NewPageAsync();

        await page.GotoAsync("/Photos/Upload");

        page.Url.Should().Contain("/Account/Login");
    }

    [Fact]
    public async Task The_search_page_accepts_filters_and_reports_a_result_count()
    {
        var page = await fixture.NewPageAsync();

        await page.GotoAsync("/Gallery/Search");
        await page.FillAsync("#Hashtag", "sunset");
        await page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();

        await Assertions.Expect(page.GetByText("photo(s) found")).ToBeVisibleAsync();
    }
}
