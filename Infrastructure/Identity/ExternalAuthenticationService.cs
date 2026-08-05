using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pinkterest.Application.Accounts;
using Pinkterest.Application.Accounts.External;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.Application.Common.Events;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Packages;
using Pinkterest.Domain.Constants;
using Pinkterest.Domain.Entities;
using Pinkterest.Domain.Events;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Identity;

public sealed class ExternalAuthenticationService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context,
    IDomainEventDispatcher dispatcher,
    IAuditLog auditLog,
    TimeProvider timeProvider,
    ILogger<ExternalAuthenticationService> logger) : IExternalAuthenticationService
{
    public async Task<IReadOnlyList<ExternalProvider>> GetProvidersAsync()
    {
        var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();

        return schemes
            .Select(scheme => new ExternalProvider(scheme.Name, scheme.DisplayName ?? scheme.Name))
            .ToList();
    }

    public async Task<ExternalLoginResult> TrySignInAsync(CancellationToken cancellationToken = default)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();

        if (info is null)
        {
            return ExternalLoginResult.Failed(ExternalAuthErrors.NoLoginInfo);
        }

        var attempt = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (attempt.IsLockedOut)
        {
            return new ExternalLoginResult(
                ExternalLoginOutcome.LockedOut, info.LoginProvider, Error: AccountErrors.LockedOut);
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);

        if (attempt.Succeeded)
        {
            await auditLog.RecordAsync(
                new AuditEntry(AuditActions.Login, Details: new { Provider = info.LoginProvider }, UserName: email),
                cancellationToken);

            return new ExternalLoginResult(ExternalLoginOutcome.SignedIn, info.LoginProvider, email);
        }

        var displayName =
            info.Principal.FindFirstValue(ClaimTypes.Name)
            ?? info.Principal.FindFirstValue(ClaimTypes.GivenName)
            ?? email?.Split('@')[0];

        return new ExternalLoginResult(
            ExternalLoginOutcome.RequiresRegistration, info.LoginProvider, email, displayName);
    }

    public async Task<Result<Guid>> CompleteRegistrationAsync(
        string displayName,
        string email,
        Guid packageId,
        CancellationToken cancellationToken = default)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();

        if (info is null)
        {
            return Result.Failure<Guid>(ExternalAuthErrors.NoLoginInfo);
        }

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Result.Failure<Guid>(ExternalAuthErrors.EmailAlreadyRegistered);
        }

        var packageExists = await context.Packages
            .AnyAsync(package => package.Id == packageId, cancellationToken);

        if (!packageExists)
        {
            return Result.Failure<Guid>(PackageErrors.Unknown);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName,
            PackageId = packageId,
            CreatedUtc = timeProvider.GetUtcNow()
        };

        var creation = await userManager.CreateAsync(user);

        if (!creation.Succeeded)
        {
            var details = string.Join(" ", creation.Errors.Select(error => error.Description));
            return Result.Failure<Guid>(AccountErrors.RegistrationFailed(details));
        }

        var link = await userManager.AddLoginAsync(user, info);

        if (!link.Succeeded)
        {
            await userManager.DeleteAsync(user);
            var details = string.Join(" ", link.Errors.Select(error => error.Description));
            return Result.Failure<Guid>(ExternalAuthErrors.LinkFailed(details));
        }

        await userManager.AddToRoleAsync(user, Roles.RegisteredUser);
        await signInManager.SignInAsync(user, isPersistent: false);

        await dispatcher.PublishAsync(
            new UserRegisteredEvent(user.Id, email, packageId, timeProvider.GetUtcNow()),
            cancellationToken);

        logger.LogInformation("Registered user {UserId} through {Provider}.", user.Id, info.LoginProvider);
        return Result.Success(user.Id);
    }
}
