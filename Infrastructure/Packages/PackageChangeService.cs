using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pinkterest.Application.Common.Events;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Packages;
using Pinkterest.Application.Packages.State;
using Pinkterest.Domain.Events;
using Pinkterest.Infrastructure.Persistence;

using Pinkterest.Application.Common.Auditing;

using Pinkterest.CrossCutting.Auditing;

using Pinkterest.CrossCutting.Metrics;

namespace Pinkterest.Infrastructure.Packages;

public sealed class PackageChangeService(
    ApplicationDbContext context,
    IDomainEventDispatcher dispatcher,
    TimeProvider timeProvider,
    ILogger<PackageChangeService> logger) : IPackageChangeService
{
    [Audited(AuditActions.PackageChangeRequested, EntityType = "ApplicationUser")]
    [Measured(AuditActions.PackageChangeRequested)]
    public async Task<Result<PackageChangePlan>> RequestChangeAsync(
        Guid userId,
        Guid targetPackageId,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<PackageChangePlan>(Error.NotFound("User"));
        }

        var target = await context.Packages
            .SingleOrDefaultAsync(p => p.Id == targetPackageId, cancellationToken);

        if (target is null)
        {
            return Result.Failure<PackageChangePlan>(PackageErrors.Unknown);
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var state = PackageSubscriptionStateFactory.For(user, today);
        var outcome = state.RequestChange(user, target, today);

        if (outcome.IsFailure)
        {
            return outcome;
        }

        var plan = outcome.Value;
        var currentPackageId = user.PackageId;

        user.PendingPackageId = plan.TargetPackageId;
        user.PendingPackageEffectiveDate = plan.EffectiveDate;
        user.LastPackageChangeUtc = timeProvider.GetUtcNow();

        await context.SaveChangesAsync(cancellationToken);

        await dispatcher.PublishAsync(
            new PackageChangeRequestedEvent(
                user.Id, currentPackageId, plan.TargetPackageId, plan.EffectiveDate, timeProvider.GetUtcNow()),
            cancellationToken);

        return outcome;
    }

    [Audited(AuditActions.PackageChangeApplied)]
    public async Task<int> ApplyDueChangesAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var due = await context.Users
            .Where(u => u.PendingPackageId != null && u.PendingPackageEffectiveDate <= today)
            .ToListAsync(cancellationToken);

        if (due.Count == 0)
        {
            return 0;
        }

        foreach (var user in due)
        {
            user.PackageId = user.PendingPackageId!.Value;
            user.PendingPackageId = null;
            user.PendingPackageEffectiveDate = null;
        }

        await context.SaveChangesAsync(cancellationToken);

        foreach (var user in due)
        {
            await dispatcher.PublishAsync(
                new PackageChangeAppliedEvent(user.Id, user.PackageId, timeProvider.GetUtcNow()),
                cancellationToken);
        }

        logger.LogInformation("Applied {Count} scheduled package change(s).", due.Count);
        return due.Count;
    }
}
