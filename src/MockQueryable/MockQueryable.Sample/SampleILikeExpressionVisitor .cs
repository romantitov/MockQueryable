using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace MockQueryable.Sample
{
  public class SampleILikeExpressionVisitor : ExpressionVisitor
  {
    private static readonly char[] value = ['%'];

    /// <summary>
    /// Rewrites the ILike method call to a string.Contains call with a trimmed
    /// and lowercased pattern.
    /// This is used to simulate the behavior of ILike in PostgreSQL, which is
    /// case-insensitive and allows for wildcard matching with '%'.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      if (node.Method.Name != nameof(NpgsqlDbFunctionsExtensions.ILike) ||
          node.Arguments.Count != 3)
      {
        return base.VisitMethodCall(node);
      }

      var stringExpression = node.Arguments[1];
      var patternExpression = node.Arguments[2];

      var toLowerPattern = Expression.Call(patternExpression, nameof(string.ToLower), Type.EmptyTypes);
      var trimMethod = typeof(string).GetMethod(nameof(string.Trim), [typeof(char[])]);
      var trimmedPattern = Expression.Call(toLowerPattern, trimMethod!, Expression.Constant(value));

      return Expression.Call(
          Expression.Call(stringExpression, nameof(string.ToLower), Type.EmptyTypes),
          nameof(string.Contains),
          Type.EmptyTypes,
          trimmedPattern
      );
    }
  }
}
