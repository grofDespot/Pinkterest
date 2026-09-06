using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Photos.Import;

namespace Pinkterest.Infrastructure.Photos.Import;

public sealed class RemoteImageFetcher(HttpClient httpClient, ILogger<RemoteImageFetcher> logger)
    : IRemoteImageFetcher
{
    private const long MaxBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/bmp" };

    public async Task<Result<RemoteImage>> FetchAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return Result.Failure<RemoteImage>(ImageImportErrors.InvalidUrl);
        }

        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            return Result.Failure<RemoteImage>(ImageImportErrors.SchemeNotAllowed);
        }

        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(uri.Host, cancellationToken);
        }
        catch (SocketException)
        {
            return Result.Failure<RemoteImage>(ImageImportErrors.Unreachable);
        }

        if (addresses.Length == 0 || addresses.Any(IsBlocked))
        {
            logger.LogWarning("Blocked image import to {Host}: resolves to a disallowed address.", uri.Host);
            return Result.Failure<RemoteImage>(ImageImportErrors.BlockedAddress);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<RemoteImage>(ImageImportErrors.Unreachable);
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            if (!AllowedContentTypes.Contains(contentType))
            {
                return Result.Failure<RemoteImage>(ImageImportErrors.NotAnImage);
            }

            if (response.Content.Headers.ContentLength is > MaxBytes)
            {
                return Result.Failure<RemoteImage>(ImageImportErrors.TooLarge);
            }

            var bytes = await ReadCappedAsync(response, cancellationToken);

            return bytes is null
                ? Result.Failure<RemoteImage>(ImageImportErrors.TooLarge)
                : Result.Success(new RemoteImage(bytes, contentType));
        }
        catch (HttpRequestException)
        {
            return Result.Failure<RemoteImage>(ImageImportErrors.Unreachable);
        }
        catch (TaskCanceledException)
        {
            return Result.Failure<RemoteImage>(ImageImportErrors.Unreachable);
        }
    }

    private static async Task<byte[]?> ReadCappedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;

        while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaxBytes)
            {
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    private static bool IsBlocked(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return address.IsIPv6LinkLocal
                || address.IsIPv6SiteLocal
                || address.IsIPv6UniqueLocal
                || (address.IsIPv4MappedToIPv6 && IsBlocked(address.MapToIPv4()));
        }

        var octets = address.GetAddressBytes();

        return octets[0] switch
        {
            10 => true,
            127 => true,
            169 => octets[1] == 254,
            172 => octets[1] >= 16 && octets[1] <= 31,
            192 => octets[1] == 168,
            0 => true,
            _ => false
        };
    }
}
