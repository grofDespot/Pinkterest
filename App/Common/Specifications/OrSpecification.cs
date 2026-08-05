using System.Linq.Expressions;

namespace Pinkterest.Application.Common.Specifications;

public sealed class OrSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var parameter = Expression.Parameter(typeof(T), "candidate");

        var body = Expression.OrElse(
            SpecificationExpression.Rebind(left.ToExpression(), parameter),
            SpecificationExpression.Rebind(right.ToExpression(), parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
