using System.Linq.Expressions;

namespace Pinkterest.Application.Common.Specifications;

internal sealed class ParameterReplacer(ParameterExpression source, ParameterExpression target)
    : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node) =>
        ReferenceEquals(node, source) ? target : node;
}
