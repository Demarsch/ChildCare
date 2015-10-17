namespace Core.Expressions
{
    public abstract class SearchExpressionProvider<T> : ISearchExpressionProvider<T>
    {
        protected static readonly SearchExpression<T> EmptyExpression = new SearchExpression<T>(new string[0], null);

        public abstract SearchExpression<T> CreateSearchExpression(string searchPattern);
    }
}
