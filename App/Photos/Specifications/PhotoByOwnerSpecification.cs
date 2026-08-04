using System.Linq.Expressions;
using Pinkterest.Application.Common.Specifications;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Photos.Specifications;

public sealed class PhotoByOwnerSpecification(Guid ownerId) : Specification<Photo>
{
    public override Expression<Func<Photo, bool>> ToExpression() => photo => photo.OwnerId == ownerId;
}
