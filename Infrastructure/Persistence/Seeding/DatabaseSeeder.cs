using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pinkterest.Domain.Constants;
using Pinkterest.Domain.Entities;
using Pinkterest.Domain.Enums;

namespace Pinkterest.Infrastructure.Persistence.Seeding;

public sealed class DatabaseSeeder(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<SeedOptions> options,
    TimeProvider timeProvider,
    ILogger<DatabaseSeeder> logger)
{
    private readonly SeedOptions _options = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);
        await SeedRolesAsync();
        await SeedPackagesAsync(cancellationToken);
        await SeedAdministratorAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
                logger.LogInformation("Created role {Role}.", role);
            }
        }
    }

    private async Task SeedPackagesAsync(CancellationToken cancellationToken)
    {
        var existingTiers = await context.Packages
            .Select(p => p.Tier)
            .ToListAsync(cancellationToken);

        var missing = PackageDefinitions.All
            .Where(p => !existingTiers.Contains(p.Tier))
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        context.Packages.AddRange(missing);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} package(s).", missing.Count);
    }

    private async Task SeedAdministratorAsync()
    {
        if (string.IsNullOrWhiteSpace(_options.AdministratorEmail) ||
            string.IsNullOrWhiteSpace(_options.AdministratorPassword))
        {
            logger.LogWarning("Administrator seed credentials are not configured. Skipping administrator creation.");
            return;
        }

        if (await userManager.FindByEmailAsync(_options.AdministratorEmail) is not null)
        {
            return;
        }

        var goldPackage = await context.Packages.SingleAsync(p => p.Tier == PackageTier.Gold);

        var administrator = new ApplicationUser
        {
            UserName = _options.AdministratorEmail,
            Email = _options.AdministratorEmail,
            EmailConfirmed = true,
            DisplayName = _options.AdministratorDisplayName,
            PackageId = goldPackage.Id,
            CreatedUtc = timeProvider.GetUtcNow()
        };

        var result = await userManager.CreateAsync(administrator, _options.AdministratorPassword);

        if (!result.Succeeded)
        {
            logger.LogError(
                "Failed to create the administrator account: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRolesAsync(administrator, [Roles.Administrator, Roles.RegisteredUser]);
        logger.LogInformation("Created administrator account {Email}.", _options.AdministratorEmail);
    }
}
