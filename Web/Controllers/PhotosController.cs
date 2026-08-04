using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pinkterest.Application.Common.Interfaces;
using Pinkterest.Application.Photos;
using Pinkterest.Application.Photos.Processing;
using Pinkterest.Web.Models.Photos;

namespace Pinkterest.Web.Controllers;

[Authorize]
public class PhotosController(
    IPhotoUploadService uploadService,
    ICurrentUser currentUser) : Controller
{
    private const long MaxRequestBytes = 64L * 1024 * 1024;

    [HttpGet]
    public IActionResult Upload() => View(new UploadPhotoViewModel());

    [HttpPost]
    [RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> Upload(UploadPhotoViewModel model, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Forbid();
        }

        if (!ModelState.IsValid || model.File is null)
        {
            return View(model);
        }

        await using var content = model.File.OpenReadStream();

        var request = new UploadPhotoRequest(
            userId,
            model.File.FileName,
            model.File.ContentType,
            model.File.Length,
            content,
            model.Description,
            HashtagNormalizer.Parse(model.Hashtags),
            new ImageProcessingOptions(model.Format, model.MaxWidth, model.MaxHeight, model.SelectedFilters));

        var result = await uploadService.UploadAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(model);
        }

        TempData["UploadedPhotoId"] = result.Value.ToString();
        return RedirectToAction(nameof(Upload));
    }
}
