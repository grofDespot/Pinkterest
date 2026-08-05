using System.Linq.Expressions;

namespace Pinkterest.Application.Common.Specifications;

internal static class SpecificationExpression
{
    public static Expression Rebind<T>(Expression<Func<T, bool>> expression, ParameterExpression parameter) =>
        new ParameterReplacer(expression.Parameters[0], parameter).Visit(expression.Body)!;
}
