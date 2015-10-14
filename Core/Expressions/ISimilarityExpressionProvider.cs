using System;
using System.Linq.Expressions;

namespace Core.Expressions
{
    public interface ISimilarityExpressionProvider<T>
    {
        Expression<Func<T, int>> CreateSimilarityExpression(string searchPattern);
    }
}
