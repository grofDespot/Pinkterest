using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pinkterest.Application.Photos;
using Pinkterest.Application.Photos.Import;
using Pinkterest.Application.Photos.Processing;
using Pinkterest.Web.Models.Api;

namespace Pinkterest.Web.Controllers.Api;

[ApiController]
[Route("api/photos/import")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ImageImportController(
    IRemoteImageFetcher fetcher,
    IPhotoUploadService uploadService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Import(ImportImageRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var ownerId))
        {
            return Forbid();
        }

        var fetched = await fetcher.FetchAsync(request.Url, cancellationToken);

        if (fetched.IsFailure)
        {
            return BadRequest(new { error = fetched.Error.Message });
        }

        using var content = new MemoryStream(fetched.Value.Content);

        var upload = new UploadPhotoRequest(
            ownerId,
            FileName: "imported",
            fetched.Value.ContentType,
            fetched.Value.Content.Length,
            content,
            request.Description ?? string.Empty,
            [],
            new ImageProcessingOptions(ImageOutputFormat.Original, null, null, []));

        var result = await uploadService.UploadAsync(upload, cancellationToken);

        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error.Message });
    }
}
