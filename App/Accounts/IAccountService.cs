using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Accounts;

public interface IAccountService
{
    Task<Result<Guid>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<Result> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync();
}
