namespace Pinkterest.Application.Photos.Download;

public sealed record PhotoDownload(Stream Content, string ContentType, string FileName) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
