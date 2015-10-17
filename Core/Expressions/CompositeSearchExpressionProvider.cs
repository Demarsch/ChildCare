using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Expressions
{
    public class CompositeSearchExpressionProvider<T> : ISearchExpressionProvider<T>
    {
        private readonly IEnumerable<ISearchExpressionProvider<T>> providers;

        public CompositeSearchExpressionProvider(IEnumerable<ISearchExpressionProvider<T>> providers)
        {
            if (providers == null)
            {
                throw new ArgumentNullException("providers");
            }
            this.providers = providers.ToArray();
        }

        public SearchExpression<T> CreateSearchExpression(string searchPattern)
        {
            var type = typeof(T);
            var parameter = Expression.Parameter(type, type.Name.ToLower());
            var parameterReplacer = new ParameterReplacer();
            var expressions = providers.Select(x => x.CreateSearchExpression(searchPattern))
                                       .Where(x => x.SimilarityExpression != null)
                                       .ToArray();
            var expressionBodies = expressions
                .Select(x => parameterReplacer.ReplaceParameter(x.SimilarityExpression, parameter))
                .Cast<Expression<Func<T, int>>>()
                .Select(x => x.Body)
                .ToArray();
            Expression body = null;
            foreach (var expression in expressionBodies)
            {
                body = body == null ? expression : Expression.Add(body, expression);
            }
            if (body == null)
            {
                return null;
            }
            return new SearchExpression<T>(new HashSet<string>(expressions.SelectMany(x => x.ParsedTokens), StringComparer.CurrentCultureIgnoreCase),
                                               Expression.Lambda<Func<T, int>>(body, parameter));
        }
    }
}