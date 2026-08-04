using System.Linq.Expressions;

namespace Pinkterest.Application.Common.Specifications;

public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public Specification<T> And(Specification<T> other) => new AndSpecification<T>(this, other);

    public Specification<T> Or(Specification<T> other) => new OrSpecification<T>(this, other);

    public Specification<T> Not() => new NotSpecification<T>(this);

    public bool IsSatisfiedBy(T candidate) => ToExpression().Compile()(candidate);

    public static Specification<T> All { get; } = new AllSpecification<T>();
}
