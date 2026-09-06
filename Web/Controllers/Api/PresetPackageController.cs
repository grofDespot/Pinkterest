using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pinkterest.Application.Photos.Presets;

namespace Pinkterest.Web.Controllers.Api;

[ApiController]
[Route("api/photos/presets")]
[IgnoreAntiforgeryToken]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class PresetPackageController(IPresetPackageService packages) : ControllerBase
{
    private const long MaxPackageBytes = 64 * 1024;

    [HttpGet("{id:guid}/export")]
    public async Task<IActionResult> Export(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetOwner(out var ownerId))
        {
            return Forbid();
        }

        var result = await packages.ExportAsync(id, ownerId, cancellationToken);

        return result.IsSuccess
            ? File(result.Value, "application/octet-stream", $"{id}.pkpreset")
            : NotFound(new { error = result.Error.Message });
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryGetOwner(out var ownerId))
        {
            return Forbid();
        }

        if (file is null || file.Length == 0 || file.Length > MaxPackageBytes)
        {
            return BadRequest(new { error = "Provide a preset package no larger than 64 KB." });
        }

        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);

        var result = await packages.ImportAsync(ownerId, buffer.ToArray(), cancellationToken);

        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error.Message });
    }

    private bool TryGetOwner(out Guid ownerId)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(subject, out ownerId);
    }
}
