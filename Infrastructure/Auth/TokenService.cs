using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pinkterest.Application.Auth;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.Application.Common.Results;
using Pinkterest.CrossCutting.Auditing;
using Pinkterest.Domain.Entities;
using Pinkterest.Infrastructure.Identity;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Auth;

public sealed class TokenService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context,
    IAuditLog auditLog,
    IOptions<JwtOptions> options,
    TimeProvider timeProvider,
    ILogger<TokenService> logger) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    [Audited(AuditActions.TokenIssued)]
    public async Task<Result<TokenPair>> IssueAsync(
        string email,
        string password,
        string? clientAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Result.Failure<TokenPair>(AuthErrors.InvalidCredentials);
        }

        var attempt = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (!attempt.Succeeded)
        {
            return Result.Failure<TokenPair>(AuthErrors.InvalidCredentials);
        }

        return Result.Success(await IssuePairAsync(user, clientAddress, cancellationToken));
    }

    [Audited(AuditActions.TokenRefreshed)]
    public async Task<Result<TokenPair>> RefreshAsync(
        string refreshToken,
        string? clientAddress,
        CancellationToken cancellationToken = default)
    {
        var hash = Hash(refreshToken);

        var stored = await context.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);

        if (stored is null)
        {
            return Result.Failure<TokenPair>(AuthErrors.InvalidRefreshToken);
        }

        if (stored.RevokedUtc is not null)
        {
            await RevokeDescendantsAsync(stored, cancellationToken);

            logger.LogWarning(
                "Refresh token reuse detected for user {UserId}; descendant chain revoked.", stored.UserId);

            await auditLog.RecordAsync(
                new AuditEntry(
                    AuditActions.TokenReuseDetected,
                    Succeeded: false,
                    UserId: stored.UserId,
                    EntityType: nameof(RefreshToken),
                    EntityId: stored.Id.ToString()),
                cancellationToken);

            return Result.Failure<TokenPair>(AuthErrors.InvalidRefreshToken);
        }

        if (stored.ExpiresUtc <= timeProvider.GetUtcNow())
        {
            return Result.Failure<TokenPair>(AuthErrors.InvalidRefreshToken);
        }

        var pair = await IssuePairAsync(stored.User, clientAddress, cancellationToken, replaces: stored);

        return Result.Success(pair);
    }

    [Audited(AuditActions.TokenRevoked)]
    public async Task<Result> RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = Hash(refreshToken);

        var stored = await context.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);

        if (stored is null || stored.RevokedUtc is not null)
        {
            return Result.Failure(AuthErrors.InvalidRefreshToken);
        }

        stored.RevokedUtc = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<TokenPair> IssuePairAsync(
        ApplicationUser user,
        string? clientAddress,
        CancellationToken cancellationToken,
        RefreshToken? replaces = null)
    {
        var now = timeProvider.GetUtcNow();
        var roles = await userManager.GetRolesAsync(user);

        var accessExpires = now.AddMinutes(_options.AccessTokenMinutes);
        var accessToken = CreateAccessToken(user, roles, now, accessExpires);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var refreshExpires = now.AddDays(_options.RefreshTokenDays);
        var refreshHash = Hash(refreshToken);

        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            CreatedUtc = now,
            ExpiresUtc = refreshExpires,
            CreatedByIp = clientAddress
        });

        if (replaces is not null)
        {
            replaces.RevokedUtc = now;
            replaces.ReplacedByTokenHash = refreshHash;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new TokenPair(accessToken, refreshToken, accessExpires, refreshExpires);
    }

    private string CreateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles,
        DateTimeOffset issuedAt,
        DateTimeOffset expires)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task RevokeDescendantsAsync(RefreshToken start, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var current = start;

        while (current is not null)
        {
            current.RevokedUtc ??= now;

            var next = current.ReplacedByTokenHash;

            current = next is null
                ? null
                : await context.RefreshTokens
                    .SingleOrDefaultAsync(token => token.TokenHash == next, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
