using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pinkterest.Application.Common.Interfaces;
using Pinkterest.Application.Common.Specifications;
using Pinkterest.Application.Photos;
using Pinkterest.Application.Photos.Download;
using Pinkterest.Application.Photos.Search;
using Pinkterest.Application.Photos.Storage;
using Pinkterest.Domain.Constants;
using Pinkterest.Domain.Entities;
using Pinkterest.Web.Models.Photos;

namespace Pinkterest.Web.Controllers;

public class GalleryController(
    IPhotoRepository repository,
    IPhotoEditService editService,
    IPhotoSearchService searchService,
    IPhotoDownloadService downloadService,
    IPhotoStorage storage,
    ICurrentUser currentUser) : Controller
{
    private const int DefaultPageSize = 10;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var photos = await repository.ListAsync(
            Specification<Photo>.All, DefaultPageSize, cancellationToken: cancellationToken);

        return View(photos);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var photo = await repository.GetDetailAsync(id, cancellationToken);

        return photo is null ? NotFound() : View(photo);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(PhotoSearchViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Results = await searchService.SearchAsync(model.ToQuery(), cancellationToken);
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Download(
        Guid id,
        DownloadOptionsViewModel options,
        CancellationToken cancellationToken)
    {
        var result = await downloadService.PrepareAsync(id, options.ToProcessingOptions(), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        var download = result.Value;
        return File(download.Content, download.ContentType, download.FileName);
    }

    [HttpGet]
    [AllowAnonymous]
    public Task<IActionResult> Thumbnail(Guid id, CancellationToken cancellationToken) =>
        StreamAsync(id, thumbnail: true, download: false, cancellationToken);

    [HttpGet]
    [AllowAnonymous]
    public Task<IActionResult> Raw(Guid id, CancellationToken cancellationToken) =>
        StreamAsync(id, thumbnail: false, download: false, cancellationToken);

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var photo = await repository.GetDetailAsync(id, cancellationToken);

        if (photo is null)
        {
            return NotFound();
        }

        if (!CanEdit(photo.OwnerId))
        {
            return Forbid();
        }

        return View(new EditPhotoViewModel
        {
            Id = photo.Id,
            Description = photo.Description,
            Hashtags = string.Join(' ', photo.Hashtags)
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Edit(EditPhotoViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (currentUser.UserId is not { } editorId)
        {
            return Forbid();
        }

        var result = await editService.UpdateDetailsAsync(
            model.Id,
            editorId,
            currentUser.IsInRole(Roles.Administrator),
            model.Description,
            HashtagNormalizer.Parse(model.Hashtags),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    private async Task<IActionResult> StreamAsync(
        Guid id,
        bool thumbnail,
        bool download,
        CancellationToken cancellationToken)
    {
        var info = await repository.GetStorageInfoAsync(id, thumbnail, cancellationToken);

        if (info is not { } stored)
        {
            return NotFound();
        }

        if (!await storage.ExistsAsync(stored.StorageKey, cancellationToken))
        {
            return NotFound();
        }

        var content = await storage.OpenReadAsync(stored.StorageKey, cancellationToken);

        return download
            ? File(content, stored.ContentType, $"{id}{Path.GetExtension(stored.StorageKey)}")
            : File(content, stored.ContentType);
    }

    private bool CanEdit(Guid ownerId) =>
        currentUser.UserId == ownerId || currentUser.IsInRole(Roles.Administrator);
}
