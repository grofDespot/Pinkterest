using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Web.Controllers.Api;

// DELIBERATELY VULNERABLE ENDPOINT.
// This controller exists only to demonstrate SQL injection
[ApiController]
[AllowAnonymous]
[Route("api/legacy/photos")]
public sealed class LegacySearchController(ApplicationDbContext context, IConfiguration configuration)
    : ControllerBase
{
    [HttpGet("search-vulnerable")]
    public async Task<IActionResult> SearchVulnerable(string author, CancellationToken cancellationToken)
    {
        if (!configuration.GetValue("SecureCodingDemo:EnableVulnerableEndpoint", false))
        {
            return NotFound();
        }

        var sql = "SELECT \"Description\" FROM \"Photos\" p "
                + "JOIN \"AspNetUsers\" u ON p.\"OwnerId\" = u.\"Id\" "
                + "WHERE u.\"DisplayName\" = '" + author + "'";

        var descriptions = await context.Database
            .SqlQueryRaw<string>(sql)
            .ToListAsync(cancellationToken);

        return Ok(descriptions);
    }

    [HttpGet("search-fixed")]
    public async Task<IActionResult> SearchFixed(string author, CancellationToken cancellationToken)
    {
        if (!configuration.GetValue("SecureCodingDemo:EnableVulnerableEndpoint", false))
        {
            return NotFound();
        }

        var descriptions = await context.Database
            .SqlQuery<string>(
                $"SELECT \"Description\" FROM \"Photos\" p JOIN \"AspNetUsers\" u ON p.\"OwnerId\" = u.\"Id\" WHERE u.\"DisplayName\" = {author}")
            .ToListAsync(cancellationToken);

        return Ok(descriptions);
    }
}
