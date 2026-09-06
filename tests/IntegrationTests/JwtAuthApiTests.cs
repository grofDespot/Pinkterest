using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Pinkterest.IntegrationTests.Infrastructure;
using Xunit;

namespace Pinkterest.IntegrationTests;

[Collection(PinkterestCollection.Name)]
public class JwtAuthApiTests(PinkterestFixture fixture)
{
    [Fact]
    public async Task Valid_credentials_return_a_bearer_token_pair()
    {
        var email = await JwtTokens.RegisterUserAsync(fixture);

        var pair = await JwtTokens.LoginAsync(fixture, email, TestCredentials.UserPassword);

        pair.TokenType.Should().Be("Bearer");
        pair.AccessToken.Should().NotBeNullOrWhiteSpace();
        pair.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task A_wrong_password_is_rejected_with_401()
    {
        var email = await JwtTokens.RegisterUserAsync(fixture);

        var response = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/login", new { email, password = "Wrong!Password#2026" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unknown_account_is_rejected_with_401()
    {
        var response = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/login",
            new { email = $"ghost-{Guid.CreateVersion7():N}@pinkterest.test", password = TestCredentials.UserPassword });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_refresh_token_can_be_exchanged_for_a_new_pair()
    {
        var email = await JwtTokens.RegisterUserAsync(fixture);
        var pair = await JwtTokens.LoginAsync(fixture, email, TestCredentials.UserPassword);

        var response = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/refresh", new { refreshToken = pair.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var rotated = await response.Content.ReadFromJsonAsync<JwtTokens.TokenPairResponse>();
        rotated!.RefreshToken.Should().NotBe(pair.RefreshToken, "refresh must rotate the token");
    }

    [Fact]
    public async Task Reusing_a_rotated_refresh_token_is_rejected()
    {
        var email = await JwtTokens.RegisterUserAsync(fixture);
        var pair = await JwtTokens.LoginAsync(fixture, email, TestCredentials.UserPassword);

        var first = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/refresh", new { refreshToken = pair.RefreshToken });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var reuse = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/refresh", new { refreshToken = pair.RefreshToken });

        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "a refresh token may only be used once");
    }

    [Fact]
    public async Task A_revoked_refresh_token_can_no_longer_be_used()
    {
        var email = await JwtTokens.RegisterUserAsync(fixture);
        var pair = await JwtTokens.LoginAsync(fixture, email, TestCredentials.UserPassword);

        var revoke = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/revoke", new { refreshToken = pair.RefreshToken });
        revoke.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterRevoke = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/refresh", new { refreshToken = pair.RefreshToken });

        afterRevoke.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
