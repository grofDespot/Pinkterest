using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Accounts.External;

public interface IExternalAuthenticationService
{
    Task<IReadOnlyList<ExternalProvider>> GetProvidersAsync();

    Task<ExternalLoginResult> TrySignInAsync(CancellationToken cancellationToken = default);

    Task<Result<Guid>> CompleteRegistrationAsync(
        string displayName,
        string email,
        Guid packageId,
        CancellationToken cancellationToken = default);
}
