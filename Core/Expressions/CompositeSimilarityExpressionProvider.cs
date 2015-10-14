using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Expressions
{
    public class CompositeSimilarityExpressionProvider<T> : ISimilarityExpressionProvider<T>
    {
        private readonly IEnumerable<ISimilarityExpressionProvider<T>> providers;

        public CompositeSimilarityExpressionProvider(IEnumerable<ISimilarityExpressionProvider<T>> providers)
        {
            if (providers == null)
            {
                throw new ArgumentNullException("providers");
            }
            this.providers = providers.ToArray();
        }

        public Expression<Func<T, int>> CreateSimilarityExpression(string searchPattern)
        {
            var expressions = providers.Select(x => x.CreateSimilarityExpression(searchPattern))
                                       .Where(x => x != null)
                                       .Select(x => x.Body)
                                       .ToArray();
            Expression body = null;
            foreach (var expression in expressions)
            {
                body = body == null ? expression : Expression.Add(body, expression);
            }
            if (body == null)
            {
                return null;
            }
            var type = typeof(T);
            var parameter = Expression.Parameter(type, type.Name.ToLower());
            var parameterReplacer = new ParameterReplacer(parameter);
            parameterReplacer.Visit(parameter);
            return Expression.Lambda<Func<T, int>>(body, parameter);
        }
    }
}