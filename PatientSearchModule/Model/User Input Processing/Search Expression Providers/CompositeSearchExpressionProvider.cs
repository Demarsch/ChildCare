using System;
using System.Linq.Expressions;
using Core.Expressions;

namespace PatientSearchModule.Model
{
    public class CompositeSearchExpressionProvider<T> : ISearchExpressionProvider<T>
    {
        public Expression<Func<T, int>> CreateSearchExpression(string searchPattern)
        {
            throw new NotImplementedException();
        }
    }
}