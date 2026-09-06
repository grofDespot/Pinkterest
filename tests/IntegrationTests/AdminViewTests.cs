using System.Net;
using FluentAssertions;
using Pinkterest.IntegrationTests.Infrastructure;
using Xunit;

namespace Pinkterest.IntegrationTests;

[Collection(PinkterestCollection.Name)]
public class AdminViewTests(PinkterestFixture fixture)
{
    [Theory]
    [InlineData("/Admin")]
    [InlineData("/Admin/Users")]
    [InlineData("/Admin/Users?search=flow")]
    [InlineData("/Admin/Photos")]
    [InlineData("/Admin/AuditLog")]
    public async Task The_administrator_can_open_the_admin_pages(string path)
    {
        var admin = await WebFlows.SignInAsync(
            fixture, TestCredentials.AdministratorEmail, TestCredentials.AdministratorPassword);

        var response = await admin.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task The_administrator_can_open_a_user_detail_page()
    {
        var (_, _, userId) = await WebFlows.RegisterCookieClientAsync(fixture);

        var admin = await WebFlows.SignInAsync(
            fixture, TestCredentials.AdministratorEmail, TestCredentials.AdministratorPassword);

        var response = await admin.GetAsync($"/Admin/EditUser?id={userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
