using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pinkterest.Application.Auth;
using Pinkterest.Web.Models.Api;
using Pinkterest.Web.Security;

namespace Pinkterest.Web.Controllers.Api;

[ApiController]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[Route("api/auth")]
public sealed class AuthApiController(ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Authentication)]
    public async Task<IActionResult> Login(LoginApiRequest request, CancellationToken cancellationToken)
    {
        var result = await tokenService.IssueAsync(
            request.Email, request.Password, ClientAddress(), cancellationToken);

        return result.IsSuccess
            ? Ok(TokenResponse.From(result.Value))
            : Unauthorized(new { error = result.Error.Message });
    }

    [HttpPost("refresh")]
    [EnableRateLimiting(RateLimitPolicies.Authentication)]
    public async Task<IActionResult> Refresh(RefreshApiRequest request, CancellationToken cancellationToken)
    {
        var result = await tokenService.RefreshAsync(
            request.RefreshToken, ClientAddress(), cancellationToken);

        return result.IsSuccess
            ? Ok(TokenResponse.From(result.Value))
            : Unauthorized(new { error = result.Error.Message });
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshApiRequest request, CancellationToken cancellationToken)
    {
        var result = await tokenService.RevokeAsync(request.RefreshToken, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : Unauthorized(new { error = result.Error.Message });
    }

    private string? ClientAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
