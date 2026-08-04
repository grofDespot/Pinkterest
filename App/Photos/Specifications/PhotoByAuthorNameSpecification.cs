using System.Linq.Expressions;
using Pinkterest.Application.Common.Specifications;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Photos.Specifications;

public sealed class PhotoByAuthorNameSpecification(string authorName) : Specification<Photo>
{
    public override Expression<Func<Photo, bool>> ToExpression() =>
        photo => photo.Owner.DisplayName.ToLower().Contains(authorName.ToLower());
}
