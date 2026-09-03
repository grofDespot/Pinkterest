using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pinkterest.Application.Photos;
using Pinkterest.Application.Photos.Specifications;

namespace Pinkterest.Web.Controllers.Api;

[ApiController]
[Route("api/photos")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class PhotosApiController(IPhotoRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(subject, out var ownerId))
        {
            return Forbid();
        }

        var photos = await repository.ListAsync(
            new PhotoByOwnerSpecification(ownerId), take: 50, cancellationToken: cancellationToken);

        return Ok(photos.Select(photo => new
        {
            photo.Id,
            photo.Description,
            photo.UploadedUtc,
            photo.Width,
            photo.Height,
            photo.SizeBytes,
            photo.Hashtags
        }));
    }
}
