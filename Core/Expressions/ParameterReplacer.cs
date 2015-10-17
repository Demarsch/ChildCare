using System.Linq.Expressions;

namespace Core.Expressions
{
    public class ParameterReplacer : ExpressionVisitor
    {
        private readonly object replaceLock = new object();

        private ParameterExpression sourceParameter;

        private ParameterExpression newParameter;

        public Expression ReplaceParameter(Expression lambda, ParameterExpression newParameter)
        {
            lock (replaceLock)
            {
                this.newParameter = newParameter;
                sourceParameter = null;
                return Visit(lambda);
            }
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (sourceParameter == null)
            {
                sourceParameter = node;
                return newParameter;
            }
            return node.Name == sourceParameter.Name ? newParameter : node;
        }
    }
}
