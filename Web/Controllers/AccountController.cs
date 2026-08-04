using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pinkterest.Application.Accounts;
using Pinkterest.Application.Common.Interfaces;
using Pinkterest.Application.Packages;
using Pinkterest.Application.Usage;
using Pinkterest.Web.Models.Account;

namespace Pinkterest.Web.Controllers;

public class AccountController(
    IAccountService accountService,
    IPackageCatalog packageCatalog,
    IUsageQuery usageQuery,
    ICurrentUser currentUser) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Register(CancellationToken cancellationToken)
    {
        if (currentUser.IsAuthenticated)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        return View(new RegisterViewModel { Packages = await packageCatalog.GetAllAsync(cancellationToken) });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Packages = await packageCatalog.GetAllAsync(cancellationToken);
            return View(model);
        }

        var request = new RegisterRequest(model.DisplayName, model.Email, model.Password, model.PackageId);
        var result = await accountService.RegisterAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            model.Packages = await packageCatalog.GetAllAsync(cancellationToken);
            return View(model);
        }

        return RedirectToAction(nameof(Usage));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (currentUser.IsAuthenticated)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await accountService.LoginAsync(
            new LoginRequest(model.Email, model.Password, model.RememberMe), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(model);
        }

        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await accountService.LogoutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Usage(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Forbid();
        }

        var result = await usageQuery.GetForUserAsync(userId, cancellationToken);

        return result.IsSuccess ? View(result.Value) : NotFound();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction(nameof(HomeController.Index), "Home");
}
