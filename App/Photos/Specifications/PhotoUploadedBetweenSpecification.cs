using System.Linq.Expressions;
using Pinkterest.Application.Common.Specifications;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Photos.Specifications;

public sealed class PhotoUploadedBetweenSpecification(DateTimeOffset? from, DateTimeOffset? to)
    : Specification<Photo>
{
    public override Expression<Func<Photo, bool>> ToExpression() =>
        photo => (from == null || photo.UploadedUtc >= from)
                 && (to == null || photo.UploadedUtc <= to);
}
