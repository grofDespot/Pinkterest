using System.Linq.Expressions;

namespace Pinkterest.Application.Common.Specifications;

public sealed class AllSpecification<T> : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression() => _ => true;
}
