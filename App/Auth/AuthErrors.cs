using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Auth;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        new("Api.InvalidCredentials", "The email address or password is incorrect, or the account is temporarily locked.");

    public static readonly Error InvalidRefreshToken =
        new("Api.InvalidRefreshToken", "The refresh token is not valid.");
}
