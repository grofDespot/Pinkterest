using System.Linq.Expressions;

namespace Pinkterest.Application.Common.Specifications;

public sealed class NotSpecification<T>(Specification<T> inner) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var parameter = Expression.Parameter(typeof(T), "candidate");
        var body = Expression.Not(SpecificationExpression.Rebind(inner.ToExpression(), parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
