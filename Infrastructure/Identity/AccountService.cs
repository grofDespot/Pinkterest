using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pinkterest.Application.Accounts;
using Pinkterest.Application.Common.Results;
using Pinkterest.Domain.Constants;
using Pinkterest.Domain.Entities;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Identity;

public sealed class AccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context,
    TimeProvider timeProvider,
    ILogger<AccountService> logger) : IAccountService
{
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

        logger.LogInformation("Registered user {UserId}.", user.Id);
        return Result.Success(user.Id);
    }

    public async Task<Result> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Result.Failure(AccountErrors.InvalidCredentials);
        }

        var attempt = await signInManager.PasswordSignInAsync(
            user, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (attempt.IsLockedOut)
        {
            logger.LogWarning("Login blocked for locked out user {UserId}.", user.Id);
            return Result.Failure(AccountErrors.LockedOut);
        }

        return attempt.Succeeded
            ? Result.Success()
            : Result.Failure(AccountErrors.InvalidCredentials);
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();
}
