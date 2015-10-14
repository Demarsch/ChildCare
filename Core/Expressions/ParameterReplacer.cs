using System;
using System.Linq.Expressions;

namespace Core.Expressions
{
    public class ParameterReplacer : ExpressionVisitor
    {
        public ParameterExpression Parameter { get; private set; }

        public ParameterReplacer(ParameterExpression parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }
            Parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return Parameter;
        }
    }
}
