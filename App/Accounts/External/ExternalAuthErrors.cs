using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Accounts.External;

public static class ExternalAuthErrors
{
    public static readonly Error NoLoginInfo =
        new("ExternalLogin.NoInfo", "The external sign-in did not complete. Please try again.");

    public static readonly Error EmailAlreadyRegistered =
        new("ExternalLogin.EmailTaken", "An account with that email address already exists. Sign in with your password and link the provider from your profile.");

    public static Error LinkFailed(string details) =>
        new("ExternalLogin.LinkFailed", details);
}
