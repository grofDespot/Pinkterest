namespace Pinkterest.UiTests.Infrastructure;

public static class TestUser
{
    public const string Password = "Ui!RegisteredUser#2026";

    public static string NewEmail() => $"ui-{Guid.CreateVersion7():N}@pinkterest.test";
}
