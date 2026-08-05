using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Pinkterest.Infrastructure.Persistence;
using Pinkterest.Infrastructure.Persistence.Seeding;
using Testcontainers.PostgreSql;
using Xunit;

namespace Pinkterest.IntegrationTests.Infrastructure;

public sealed class PinkterestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("pinkterest_tests")
        .WithUsername("pinkterest")
        .WithPassword("pinkterest")
        .Build();

    private string _storageRoot = string.Empty;

    public PinkterestApplicationFactory Factory { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _storageRoot = Path.Combine(Path.GetTempPath(), "pinkterest-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_storageRoot);

        Factory = new PinkterestApplicationFactory(_postgres.GetConnectionString(), _storageRoot);

        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _postgres.DisposeAsync();

        try
        {
            if (Directory.Exists(_storageRoot))
            {
                Directory.Delete(_storageRoot, recursive: true);
            }
        }
        catch (IOException)
        {
        }
    }

    public HttpClient CreateClient(bool followRedirects = false) =>
        Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = followRedirects,
            HandleCookies = true
        });

    public async Task<T> UseDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> work)
    {
        using var scope = Factory.Services.CreateScope();
        return await work(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }
}

[CollectionDefinition(Name)]
public sealed class PinkterestCollection : ICollectionFixture<PinkterestFixture>
{
    public const string Name = "pinkterest";
}
