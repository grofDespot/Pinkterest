using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pinkterest.Application.Common.Interfaces;
using Pinkterest.Application.Photos;
using Pinkterest.Application.Photos.Presets;
using Pinkterest.Application.Photos.Processing;
using Pinkterest.Web.Models.Photos;

namespace Pinkterest.Web.Controllers;

[Authorize]
public class PhotosController(
    IPhotoUploadService uploadService,
    IFilterPresetService presetService,
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

    [HttpGet]
    public async Task<IActionResult> Presets(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Forbid();
        }

        return View(new PresetListViewModel
        {
            Presets = await presetService.ListAsync(userId, cancellationToken)
        });
    }

    [HttpPost]
    public async Task<IActionResult> SavePreset(SavePresetViewModel model, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TempData["PresetError"] = "Give the preset a name.";
            return RedirectToAction("Details", "Gallery", new { id = model.PhotoId });
        }

        var result = await presetService.SaveAsync(
            userId, model.Name, model.ToProcessingOptions(), cancellationToken);

        TempData[result.IsSuccess ? "PresetSaved" : "PresetError"] =
            result.IsSuccess ? model.Name : result.Error.Message;

        return RedirectToAction("Details", "Gallery", new { id = model.PhotoId });
    }

    [HttpPost]
    public async Task<IActionResult> DeletePreset(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Forbid();
        }

        var result = await presetService.DeleteAsync(id, userId, cancellationToken);

        if (result.IsFailure)
        {
            TempData["PresetError"] = result.Error.Message;
        }

        return RedirectToAction(nameof(Presets));
    }
}
