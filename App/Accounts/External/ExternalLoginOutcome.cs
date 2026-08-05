namespace Pinkterest.Application.Accounts.External;

public enum ExternalLoginOutcome
{
    Failed = 0,
    SignedIn = 1,
    RequiresRegistration = 2,
    LockedOut = 3
}
