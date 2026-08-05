namespace Pinkterest.Application.Packages.State;

public sealed record PackageChangePlan(Guid TargetPackageId, DateOnly EffectiveDate);
