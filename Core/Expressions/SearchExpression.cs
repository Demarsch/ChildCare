using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Core.Expressions
{
    public class SearchExpression<T>
    {
        public SearchExpression(IEnumerable<string> parsedTokens, Expression<Func<T, int>> similarityExpression)
        {
            ParsedTokens = parsedTokens ?? new string[0];
            SimilarityExpression = similarityExpression;
            FilterExpression = similarityExpression == null 
                ? null 
                : Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(similarityExpression.Body, Expression.Constant(0)), SimilarityExpression.Parameters);
        }

        public IEnumerable<string> ParsedTokens { get; private set; }

        public Expression<Func<T, int>> SimilarityExpression { get; private set; }

        public Expression<Func<T, bool>>  FilterExpression { get; private set; }
    }
}
