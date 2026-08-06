using System.Linq.Expressions;
using Pinkterest.Application.Common.Specifications;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Photos.Specifications;

public sealed class PhotoByAuthorNameSpecification : Specification<Photo>
{
    private readonly string _authorName;

    public PhotoByAuthorNameSpecification(string authorName) =>
        _authorName = authorName.ToLowerInvariant();

    public override Expression<Func<Photo, bool>> ToExpression()
    {
        var authorName = _authorName;

        return photo => photo.Owner.DisplayName.ToLower().Contains(authorName);
    }
}
