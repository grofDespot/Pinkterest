using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pinkterest.Domain.Enums;

namespace Pinkterest.IntegrationTests.Infrastructure;

public static class JwtTokens
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<string> RegisterUserAsync(PinkterestFixture fixture)
    {
        var email = $"api-{Guid.CreateVersion7():N}@pinkterest.test";
        var client = fixture.CreateClient();

        var freePackageId = await fixture.UseDbContextAsync(context =>
            context.Packages.Where(p => p.Tier == PackageTier.Free).Select(p => p.Id).SingleAsync());

        await client.PostFormAsync("/Account/Register", new Dictionary<string, string>
        {
            ["DisplayName"] = "Api user",
            ["Email"] = email,
            ["Password"] = TestCredentials.UserPassword,
            ["ConfirmPassword"] = TestCredentials.UserPassword,
            ["PackageId"] = freePackageId.ToString()
        });

        return email;
    }

    public static async Task<string> RegisterAndIssueAccessTokenAsync(PinkterestFixture fixture)
    {
        var email = await RegisterUserAsync(fixture);
        var pair = await LoginAsync(fixture, email, TestCredentials.UserPassword);
        return pair.AccessToken;
    }

    public static async Task<TokenPairResponse> LoginAsync(
        PinkterestFixture fixture, string email, string password)
    {
        var response = await fixture.CreateClient().PostAsJsonAsync(
            "/api/auth/login", new { email, password });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TokenPairResponse>(Json))!;
    }

    public static HttpClient WithBearer(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public static string ExpiredAccessToken(Guid userId) =>
        Signed(userId, TestCredentials.JwtSigningKey,
            notBefore: DateTime.UtcNow.AddMinutes(-30), expires: DateTime.UtcNow.AddMinutes(-15));

    public static string WrongKeyAccessToken(Guid userId) =>
        Signed(userId, "a-different-signing-key-that-server-will-reject-000",
            notBefore: DateTime.UtcNow, expires: DateTime.UtcNow.AddMinutes(15));

    private static string Signed(Guid userId, string key, DateTime notBefore, DateTime expires)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestCredentials.JwtIssuer,
            audience: TestCredentials.JwtAudience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())],
            notBefore: notBefore,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public sealed record TokenPairResponse(
        string AccessToken,
        string RefreshToken,
        string TokenType);
}
