using System.Security.Cryptography;

namespace Pinkterest.Web.Security;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    internal const string NonceKey = "csp-nonce";

    public async Task InvokeAsync(HttpContext context)
    {
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        context.Items[NonceKey] = nonce;

        var headers = context.Response.Headers;

        headers.ContentSecurityPolicy =
            "default-src 'self'; " +
            "base-uri 'self'; " +
            "object-src 'none'; " +
            "frame-ancestors 'none'; " +
            "form-action 'self' https://accounts.google.com https://github.com; " +
            "img-src 'self' data:; " +
            $"script-src 'self' 'nonce-{nonce}'; " +
            $"style-src 'self' 'nonce-{nonce}'";

        headers.XFrameOptions = "DENY";
        headers.XContentTypeOptions = "nosniff";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), geolocation=(), microphone=(), payment=()";

        await next(context);
    }
}

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();

    public static string CspNonce(this HttpContext context) =>
        context.Items[SecurityHeadersMiddleware.NonceKey] as string ?? string.Empty;
}
