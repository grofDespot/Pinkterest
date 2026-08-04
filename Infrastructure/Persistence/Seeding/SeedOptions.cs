namespace Pinkterest.Infrastructure.Persistence.Seeding;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdministratorEmail { get; set; } = string.Empty;

    public string AdministratorPassword { get; set; } = string.Empty;

    public string AdministratorDisplayName { get; set; } = "Administrator";
}
