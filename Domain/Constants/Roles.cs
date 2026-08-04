namespace Pinkterest.Domain.Constants;

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string RegisteredUser = "RegisteredUser";

    public static readonly IReadOnlyList<string> All = [Administrator, RegisteredUser];
}
