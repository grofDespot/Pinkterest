using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Packages;

public static class PackageErrors
{
    public static readonly Error Unknown =
        new("Package.Unknown", "That package does not exist.");

    public static readonly Error AlreadyOnPackage =
        new("Package.AlreadyActive", "You are already on that package.");

    public static readonly Error ChangeAlreadyScheduled =
        new("Package.ChangeScheduled", "A package change is already scheduled. You can request another one tomorrow.");

    public static readonly Error AlreadyChangedToday =
        new("Package.AlreadyChangedToday", "You have already changed your package today. Try again tomorrow.");
}
