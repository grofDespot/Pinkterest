namespace Pinkterest.Infrastructure.Identity;

public sealed class ExternalAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public ProviderCredentials Google { get; set; } = new();

    public ProviderCredentials GitHub { get; set; } = new();
}

public sealed class ProviderCredentials
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}
