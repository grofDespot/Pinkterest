using Pinkterest.Application.Photos.Queries;

namespace Pinkterest.Application.Photos.Search;

public sealed record PhotoSearchResult(
    IReadOnlyList<PhotoSummary> Photos,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}
