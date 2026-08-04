namespace Pinkterest.Application.Photos.Processing;

public sealed record ProcessedImage(
    Stream Content,
    string ContentType,
    string FileExtension,
    int Width,
    int Height,
    long SizeBytes) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
