using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Usage;

public interface IUsageQuery
{
    Task<Result<UsageSummary>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
