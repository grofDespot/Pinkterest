using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Import;

public sealed record RemoteImage(byte[] Content, string ContentType);

public interface IRemoteImageFetcher
{
    Task<Result<RemoteImage>> FetchAsync(string url, CancellationToken cancellationToken = default);
}
