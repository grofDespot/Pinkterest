namespace Pinkterest.Application.Photos.Search;

public interface IPhotoSearchService
{
    Task<PhotoSearchResult> SearchAsync(PhotoSearchQuery query, CancellationToken cancellationToken = default);
}
