using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Auth;

public interface ITokenService
{
    Task<Result<TokenPair>> IssueAsync(
        string email,
        string password,
        string? clientAddress,
        CancellationToken cancellationToken = default);

    Task<Result<TokenPair>> RefreshAsync(
        string refreshToken,
        string? clientAddress,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
}
