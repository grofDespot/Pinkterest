using System.Linq.Expressions;
using Pinkterest.Application.Common.Specifications;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Application.Photos.Specifications;

public sealed class PhotoSizeBetweenSpecification(long? minBytes, long? maxBytes) : Specification<Photo>
{
    public override Expression<Func<Photo, bool>> ToExpression() =>
        photo => (minBytes == null || photo.SizeBytes >= minBytes)
                 && (maxBytes == null || photo.SizeBytes <= maxBytes);
}
