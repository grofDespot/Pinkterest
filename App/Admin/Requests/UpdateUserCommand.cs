using Pinkterest.Application.Common.Mediation;
using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Admin.Requests;

public sealed record UpdateUserCommand(
    Guid UserId,
    string DisplayName,
    Guid PackageId,
    bool ClearLockout) : IRequest<Result>;
