using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pinkterest.Application.Admin.Requests;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.Application.Common.Mediation;
using Pinkterest.Application.Packages;
using Pinkterest.Domain.Constants;
using Pinkterest.Web.Models.Admin;

namespace Pinkterest.Web.Controllers;

[Authorize(Policy = Policies.IsAdministrator)]
public class AdminController(
    ISender sender,
    IPackageCatalog packageCatalog,
    IAuditLog auditLog) : AuditedController(auditLog)
{
    protected override string AuditArea => "admin";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await sender.SendAsync(new GetAdminStatisticsQuery(), cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Users(string? search, CancellationToken cancellationToken)
    {
        ViewData["Search"] = search;
        return View(await sender.SendAsync(new GetUsersQuery(search), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(Guid id, CancellationToken cancellationToken)
    {
        var detail = await sender.SendAsync(new GetUserDetailQuery(id), cancellationToken);

        if (detail is null)
        {
            return NotFound();
        }

        return View(new EditUserViewModel
        {
            Id = detail.Id,
            DisplayName = detail.DisplayName,
            PackageId = detail.PackageId,
            Detail = detail,
            Packages = await packageCatalog.GetAllAsync(cancellationToken)
        });
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Detail = await sender.SendAsync(new GetUserDetailQuery(model.Id), cancellationToken);
            model.Packages = await packageCatalog.GetAllAsync(cancellationToken);
            return View(model);
        }

        return await ExecuteAuditedAsync(
            "user.update",
            () => sender.SendAsync(
                new UpdateUserCommand(model.Id, model.DisplayName, model.PackageId, model.ClearLockout),
                cancellationToken),
            () => RedirectToAction(nameof(Users)),
            entityType: "ApplicationUser",
            entityId: model.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> AuditLog(
        int page,
        string? actionFilter,
        string? userName,
        CancellationToken cancellationToken)
    {
        ViewData["ActionFilter"] = actionFilter;
        ViewData["UserName"] = userName;

        return View(await sender.SendAsync(
            new GetAuditLogQuery(Math.Max(1, page), 30, actionFilter, userName), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Photos(int page, string? author, CancellationToken cancellationToken)
    {
        ViewData["Author"] = author;

        return View(await sender.SendAsync(
            new GetManagedPhotosQuery(Math.Max(1, page), author), cancellationToken));
    }

    [HttpPost]
    public Task<IActionResult> DeletePhoto(Guid id, CancellationToken cancellationToken) =>
        ExecuteAuditedAsync(
            "photo.delete",
            () => sender.SendAsync(new DeletePhotoCommand(id), cancellationToken),
            () => RedirectToAction(nameof(Photos)),
            entityType: "Photo",
            entityId: id.ToString(),
            cancellationToken: cancellationToken);

    protected override IActionResult OnFailure(Pinkterest.Application.Common.Results.Error error)
    {
        TempData["AdminError"] = error.Message;
        return RedirectToAction(nameof(Index));
    }
}
