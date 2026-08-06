using Pinkterest.CrossCutting.Metrics;

namespace Pinkterest.Application.Photos.Search;

public sealed class PhotoSearchService(IPhotoRepository repository) : IPhotoSearchService
{
    private const int MaxPageSize = 60;

    [Measured("photo.search")]
    public async Task<PhotoSearchResult> SearchAsync(
        PhotoSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var specification = new PhotoSearchBuilder()
            .WithHashtag(query.Hashtag)
            .WithAuthor(query.Author)
            .WithSizeBetween(query.MinSizeBytes, query.MaxSizeBytes)
            .UploadedBetween(query.UploadedFrom, query.UploadedTo)
            .Build();

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var total = await repository.CountAsync(specification, cancellationToken);

        var photos = await repository.ListAsync(
            specification, pageSize, (page - 1) * pageSize, cancellationToken);

        return new PhotoSearchResult(photos, total, page, pageSize);
    }
}
