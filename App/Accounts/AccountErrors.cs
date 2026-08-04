using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Accounts;

public static class AccountErrors
{
    public static readonly Error InvalidCredentials =
        new("Login.InvalidCredentials", "The email address or password is incorrect.");

    public static readonly Error LockedOut =
        new("Login.LockedOut", "This account is temporarily locked after too many failed attempts. Try again later.");

    public static readonly Error UnknownPackage =
        new("Register.UnknownPackage", "Select one of the available packages.");

    public static Error RegistrationFailed(string details) => new("Register.Failed", details);
}
