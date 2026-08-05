using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Accounts.External;

public sealed record ExternalLoginResult(
    ExternalLoginOutcome Outcome,
    string? Provider = null,
    string? Email = null,
    string? SuggestedDisplayName = null,
    Error? Error = null)
{
    public static ExternalLoginResult Failed(Error error) =>
        new(ExternalLoginOutcome.Failed, Error: error);
}
