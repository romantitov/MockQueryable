using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace MockQueryable.Sample;

/// <summary>
/// This class rewrites ILike/Like method calls to simulate EntityFrameworkCore case-insensitive pattern matching in LINQ queries.
/// </summary>
public class SampleLikeExpressionVisitor : ExpressionVisitor
{
    private static readonly char[] Value = ['%'];

    /// <summary>
    /// Rewrites the ILike method call to a string.Contains call with a trimmed
    /// and lowercased pattern.
    /// This is used to simulate the behavior of ILike/Like 
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        //TODO: Use nameof(NpgsqlDbFunctionsExtensions.ILike) after upgrade to 10.0 version
        if (node.Method.Name is "ILike" or nameof(DbFunctionsExtensions.Like) &&
            node.Arguments.Count == 3)
        {

            var stringExpression = node.Arguments[1];
            var patternExpression = node.Arguments[2];

            var toLowerPattern = Expression.Call(patternExpression, nameof(string.ToLower), Type.EmptyTypes);
            var trimMethod = typeof(string).GetMethod(nameof(string.Trim), [typeof(char[])]);
            var trimmedPattern = Expression.Call(toLowerPattern, trimMethod!, Expression.Constant(Value));

            return Expression.Call(
                Expression.Call(stringExpression, nameof(string.ToLower), Type.EmptyTypes),
                nameof(string.Contains),
                Type.EmptyTypes,
                trimmedPattern
            );
        }

        return base.VisitMethodCall(node);
    }
}