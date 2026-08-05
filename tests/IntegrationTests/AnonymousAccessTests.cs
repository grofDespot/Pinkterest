using System.Net;
using FluentAssertions;
using Pinkterest.IntegrationTests.Infrastructure;
using Xunit;

namespace Pinkterest.IntegrationTests;

[Collection(PinkterestCollection.Name)]
public class AnonymousAccessTests(PinkterestFixture fixture)
{
    [Theory]
    [InlineData("/")]
    [InlineData("/Gallery")]
    [InlineData("/Gallery/Search")]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    public async Task Public_pages_are_reachable_without_signing_in(string path)
    {
        var response = await fixture.CreateClient().GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/Photos/Upload")]
    [InlineData("/Account/Usage")]
    [InlineData("/Account/ChangePackage")]
    [InlineData("/Admin")]
    [InlineData("/Admin/Users")]
    [InlineData("/Admin/AuditLog")]
    public async Task Protected_pages_send_anonymous_visitors_to_the_login_page(string path)
    {
        var response = await fixture.CreateClient().GetAsync(path);

        response.RedirectsTo("/Account/Login").Should().BeTrue(
            "anonymous users may only browse, so {0} must not be served", path);
    }

    [Fact]
    public async Task An_unknown_photo_returns_not_found()
    {
        var response = await fixture.CreateClient()
            .GetAsync($"/Gallery/Details/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
