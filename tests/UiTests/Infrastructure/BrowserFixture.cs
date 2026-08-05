using Microsoft.Playwright;
using Xunit;

namespace Pinkterest.UiTests.Infrastructure;

public sealed class BrowserFixture : IAsyncLifetime
{
    private IPlaywright _playwright = default!;

    public IBrowser Browser { get; private set; } = default!;

    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("PINKTEREST_BASE_URL") ?? "https://localhost:7061";

    public async Task InitializeAsync()
    {
        var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);

        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"Playwright could not install its browsers (exit code {exitCode}).");
        }

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true
        });

        return await context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        _playwright.Dispose();
    }
}

[CollectionDefinition(Name)]
public sealed class BrowserCollection : ICollectionFixture<BrowserFixture>
{
    public const string Name = "browser";
}
