using Pinkterest.Application.Common.Mediation;
using Pinkterest.Application.Photos.Search;

namespace Pinkterest.Application.Admin.Requests;

public sealed record GetManagedPhotosQuery(
    int Page = 1,
    string? Author = null) : IRequest<PhotoSearchResult>;
