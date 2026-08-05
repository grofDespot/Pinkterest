using System.Net;
using System.Text.RegularExpressions;

namespace Pinkterest.IntegrationTests.Infrastructure;

public static partial class HttpClientExtensions
{
    public static async Task<string> GetAntiforgeryTokenAsync(this HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        var match = AntiforgeryToken().Match(html);

        if (!match.Success)
        {
            throw new InvalidOperationException($"No antiforgery token found on {path}.");
        }

        return match.Groups["token"].Value;
    }

    public static async Task<HttpResponseMessage> PostFormAsync(
        this HttpClient client,
        string path,
        Dictionary<string, string> fields,
        string? antiforgeryToken = null)
    {
        var token = antiforgeryToken ?? await client.GetAntiforgeryTokenAsync(path);
        fields["__RequestVerificationToken"] = token;

        return await client.PostAsync(path, new FormUrlEncodedContent(fields));
    }

    public static bool RedirectsTo(this HttpResponseMessage response, string pathFragment) =>
        response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found or HttpStatusCode.SeeOther
        && response.Headers.Location is { } location
        && location.OriginalString.Contains(pathFragment, StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex("""name="__RequestVerificationToken"[^>]*value="(?<token>[^"]+)""")]
    private static partial Regex AntiforgeryToken();
}
