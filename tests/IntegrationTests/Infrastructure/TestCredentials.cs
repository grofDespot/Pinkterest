namespace Pinkterest.IntegrationTests.Infrastructure;

public static class TestCredentials
{
    public const string AdministratorEmail = "admin@pinkterest.test";
    public const string AdministratorPassword = "Test!Administrator#2026";
    public const string UserPassword = "Test!RegisteredUser#2026";

    public const string JwtSigningKey = "integration-tests-jwt-signing-key-0123456789abcdef";
    public const string JwtIssuer = "Pinkterest";
    public const string JwtAudience = "Pinkterest.Api";
    public const string PresetSigningKey = "integration-tests-preset-signing-key-0123456789ab";
}
