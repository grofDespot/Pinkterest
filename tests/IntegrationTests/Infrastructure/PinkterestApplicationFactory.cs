using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Pinkterest.IntegrationTests.Infrastructure;

public sealed class PinkterestApplicationFactory(string connectionString, string storageRoot)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("Storage:Provider", "Local");
        builder.UseSetting("Storage:LocalRootPath", storageRoot);
        builder.UseSetting("Seed:AdministratorEmail", TestCredentials.AdministratorEmail);
        builder.UseSetting("Seed:AdministratorPassword", TestCredentials.AdministratorPassword);
        builder.UseSetting("Authentication:Google:ClientId", string.Empty);
        builder.UseSetting("Authentication:Google:ClientSecret", string.Empty);
        builder.UseSetting("Authentication:GitHub:ClientId", string.Empty);
        builder.UseSetting("Authentication:GitHub:ClientSecret", string.Empty);
    }
}
