using System.Linq.Expressions;

namespace Pinkterest.Application.Common.Specifications;

internal sealed class ParameterReplacer(ParameterExpression parameter) : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node) => parameter;
}
