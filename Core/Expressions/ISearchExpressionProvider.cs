namespace Core.Expressions
{
    public interface ISearchExpressionProvider<T>
    {
        SearchExpression<T> CreateSearchExpression(string searchPattern);
    }
}
