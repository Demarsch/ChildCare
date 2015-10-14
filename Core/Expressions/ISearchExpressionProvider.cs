using System;
using System.Linq.Expressions;

namespace Core.Expressions
{
    public interface ISearchExpressionProvider<T>
    {
        Expression<Func<T, int>> CreateSearchExpression(string searchPattern);
    }
}
