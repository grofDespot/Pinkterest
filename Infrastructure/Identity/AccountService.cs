using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pinkterest.Application.Accounts;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.Application.Common.Events;
using Pinkterest.Application.Common.Results;
using Pinkterest.Domain.Constants;
using Pinkterest.Domain.Entities;
using Pinkterest.Domain.Events;
using Pinkterest.Infrastructure.Persistence;

using Pinkterest.CrossCutting.Auditing;

using Pinkterest.CrossCutting.Metrics;

namespace Pinkterest.Infrastructure.Identity;

public sealed class AccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context,
    IDomainEventDispatcher dispatcher,
    IAuditLog auditLog,
    TimeProvider timeProvider,
    ILogger<AccountService> logger) : IAccountService
{
    [Audited(AuditActions.Register, EntityType = nameof(ApplicationUser))]
    [Measured(AuditActions.Register)]
    public async Task<Result<Guid>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var packageExists = await context.Packages
            .AnyAsync(p => p.Id == request.PackageId, cancellationToken);

        if (!packageExists)
        {
            return Result.Failure<Guid>(AccountErrors.UnknownPackage);
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            DisplayName = request.DisplayName,
            PackageId = request.PackageId,
            CreatedUtc = timeProvider.GetUtcNow()
        };

        var creation = await userManager.CreateAsync(user, request.Password);

        if (!creation.Succeeded)
        {
            var details = string.Join(" ", creation.Errors.Select(e => e.Description));
            return Result.Failure<Guid>(AccountErrors.RegistrationFailed(details));
        }

        await userManager.AddToRoleAsync(user, Roles.RegisteredUser);
        await signInManager.SignInAsync(user, isPersistent: false);

        await dispatcher.PublishAsync(
            new UserRegisteredEvent(user.Id, user.Email!, user.PackageId, timeProvider.GetUtcNow()),
            cancellationToken);

        logger.LogInformation("Registered user {UserId}.", user.Id);
        return Result.Success(user.Id);
    }

    public async Task<Result> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            await auditLog.RecordAsync(
                new AuditEntry(AuditActions.LoginFailed, Succeeded: false, UserName: request.Email),
                cancellationToken);

            return Result.Failure(AccountErrors.InvalidCredentials);
        }

        var attempt = await signInManager.PasswordSignInAsync(
            user, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (attempt.IsLockedOut)
        {
            logger.LogWarning("Login blocked for locked out user {UserId}.", user.Id);

            await auditLog.RecordAsync(
                new AuditEntry(AuditActions.LoginFailed, Succeeded: false, UserId: user.Id, UserName: user.Email),
                cancellationToken);

            return Result.Failure(AccountErrors.LockedOut);
        }

        await auditLog.RecordAsync(
            new AuditEntry(
                attempt.Succeeded ? AuditActions.Login : AuditActions.LoginFailed,
                Succeeded: attempt.Succeeded,
                UserId: user.Id,
                UserName: user.Email),
            cancellationToken);

        return attempt.Succeeded
            ? Result.Success()
            : Result.Failure(AccountErrors.InvalidCredentials);
    }

    [Audited(AuditActions.Logout)]
    public Task LogoutAsync() => signInManager.SignOutAsync();
}
