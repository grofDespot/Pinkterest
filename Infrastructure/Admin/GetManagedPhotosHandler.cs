using Pinkterest.Application.Admin.Requests;
using Pinkterest.Application.Common.Mediation;
using Pinkterest.Application.Photos;
using Pinkterest.Application.Photos.Search;

namespace Pinkterest.Infrastructure.Admin;

public sealed class GetManagedPhotosHandler(IPhotoSearchService searchService)
    : IRequestHandler<GetManagedPhotosQuery, PhotoSearchResult>
{
    public Task<PhotoSearchResult> HandleAsync(
        GetManagedPhotosQuery request,
        CancellationToken cancellationToken = default) =>
        searchService.SearchAsync(
            new PhotoSearchQuery(Author: request.Author, Page: request.Page, PageSize: 24),
            cancellationToken);
}
