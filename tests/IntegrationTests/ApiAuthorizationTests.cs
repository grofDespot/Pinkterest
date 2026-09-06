using System.Net;
using FluentAssertions;
using Pinkterest.IntegrationTests.Infrastructure;
using Xunit;

namespace Pinkterest.IntegrationTests;

[Collection(PinkterestCollection.Name)]
public class ApiAuthorizationTests(PinkterestFixture fixture)
{
    [Fact]
    public async Task A_protected_api_call_without_a_token_is_unauthorized()
    {
        var response = await fixture.CreateClient().GetAsync("/api/photos");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_protected_api_call_with_a_valid_token_succeeds()
    {
        var token = await JwtTokens.RegisterAndIssueAccessTokenAsync(fixture);

        var response = await fixture.CreateClient().WithBearer(token).GetAsync("/api/photos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_malformed_token_is_unauthorized()
    {
        var response = await fixture.CreateClient()
            .WithBearer("not-a-real-jwt")
            .GetAsync("/api/photos");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_expired_token_is_unauthorized()
    {
        var response = await fixture.CreateClient()
            .WithBearer(JwtTokens.ExpiredAccessToken(Guid.CreateVersion7()))
            .GetAsync("/api/photos");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "ClockSkew is zero, so an expired token is refused immediately");
    }

    [Fact]
    public async Task A_token_signed_with_the_wrong_key_is_unauthorized()
    {
        var response = await fixture.CreateClient()
            .WithBearer(JwtTokens.WrongKeyAccessToken(Guid.CreateVersion7()))
            .GetAsync("/api/photos");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "a forged signature must fail issuer-signing-key validation");
    }

    [Fact]
    public async Task Exporting_a_preset_requires_authentication()
    {
        var response = await fixture.CreateClient()
            .GetAsync($"/api/photos/presets/{Guid.CreateVersion7()}/export");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Importing_a_preset_requires_authentication()
    {
        var response = await fixture.CreateClient()
            .PostAsync("/api/photos/presets/import", new MultipartFormDataContent());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_preset_the_caller_does_not_own_is_not_returned()
    {
        var token = await JwtTokens.RegisterAndIssueAccessTokenAsync(fixture);

        var response = await fixture.CreateClient()
            .WithBearer(token)
            .GetAsync($"/api/photos/presets/{Guid.CreateVersion7()}/export");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "export is owner-scoped, so another owner's id is indistinguishable from a missing one");
    }
}
