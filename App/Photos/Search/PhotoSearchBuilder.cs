using Pinkterest.Application.Common.Specifications;
using Pinkterest.Application.Photos.Specifications;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Photos.Search;

public sealed class PhotoSearchBuilder
{
    private Specification<Photo> _specification = Specification<Photo>.All;

    public PhotoSearchBuilder WithHashtag(string? hashtag)
    {
        var normalized = HashtagNormalizer.Parse(hashtag).FirstOrDefault();

        return normalized is null
            ? this
            : Add(new PhotoWithHashtagSpecification(normalized));
    }

    public PhotoSearchBuilder WithAuthor(string? author) =>
        string.IsNullOrWhiteSpace(author)
            ? this
            : Add(new PhotoByAuthorNameSpecification(author.Trim()));

    public PhotoSearchBuilder WithOwner(Guid? ownerId) =>
        ownerId is null
            ? this
            : Add(new PhotoByOwnerSpecification(ownerId.Value));

    public PhotoSearchBuilder WithSizeBetween(long? minBytes, long? maxBytes) =>
        minBytes is null && maxBytes is null
            ? this
            : Add(new PhotoSizeBetweenSpecification(minBytes, maxBytes));

    public PhotoSearchBuilder UploadedBetween(DateOnly? from, DateOnly? to)
    {
        if (from is null && to is null)
        {
            return this;
        }

        var start = from is null
            ? (DateTimeOffset?)null
            : new DateTimeOffset(from.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var end = to is null
            ? (DateTimeOffset?)null
            : new DateTimeOffset(to.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        return Add(new PhotoUploadedBetweenSpecification(start, end));
    }

    public Specification<Photo> Build() => _specification;

    private PhotoSearchBuilder Add(Specification<Photo> specification)
    {
        _specification = _specification.And(specification);
        return this;
    }
}
