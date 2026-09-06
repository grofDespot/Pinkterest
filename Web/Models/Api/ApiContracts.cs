using System.ComponentModel.DataAnnotations;
using Pinkterest.Application.Auth;

namespace Pinkterest.Web.Models.Api;

public sealed record LoginApiRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record RefreshApiRequest([Required] string RefreshToken);

public sealed record ImportImageRequest([Required] string Url, string? Description);

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    DateTimeOffset AccessTokenExpiresUtc,
    DateTimeOffset RefreshTokenExpiresUtc)
{
    public static TokenResponse From(TokenPair pair) => new(
        pair.AccessToken,
        pair.RefreshToken,
        "Bearer",
        pair.AccessTokenExpiresUtc,
        pair.RefreshTokenExpiresUtc);
}
