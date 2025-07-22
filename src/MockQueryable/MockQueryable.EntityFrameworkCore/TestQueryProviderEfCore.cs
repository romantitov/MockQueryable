using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MockQueryable.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MockQueryable.EntityFrameworkCore
{
  public class TestAsyncEnumerableEfCore<T> : TestQueryProvider<T>, IAsyncEnumerable<T>, IAsyncQueryProvider
  {
    private readonly Action<T> _optionalRemoveCallback;

    public TestAsyncEnumerableEfCore(Expression expression, Action<T> optionalRemoveCallback = null) : base(expression)
    {
      _optionalRemoveCallback = optionalRemoveCallback;
    }

    public TestAsyncEnumerableEfCore(IEnumerable<T> enumerable, Action<T> optionalRemoveCallback = null) : base(enumerable, optionalRemoveCallback)
    {
      _optionalRemoveCallback = optionalRemoveCallback;
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
      var expectedResultType = typeof(TResult).GetGenericArguments()[0];
      var executionResult = typeof(IQueryProvider)
        .GetMethods()
        .First(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
        .MakeGenericMethod(expectedResultType)
        .Invoke(this, new object[] { expression });

      return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
        .MakeGenericMethod(expectedResultType)
        .Invoke(null, new[] { executionResult });
    }

    public override TResult Execute<TResult>(Expression expression)
    {
      // Intercept ExecuteDelete and ExecuteUpdate calls
      if (expression is MethodCallExpression methodCall &&
        (methodCall.Method.Name == nameof(RelationalQueryableExtensions.ExecuteUpdate) || methodCall.Method.Name == nameof(RelationalQueryableExtensions.ExecuteDelete)) &&
        typeof(TResult) == typeof(int))
      {
        var affectedItems = base.Execute<IEnumerable<T>>(Expression).ToList();

        if (methodCall.Method.Name == nameof(RelationalQueryableExtensions.ExecuteUpdate))
        {
          ApplyUpdateChangesToDbSet(affectedItems, methodCall);
        }

        if (methodCall.Method.Name == nameof(RelationalQueryableExtensions.ExecuteDelete))
        {
          foreach (var item in affectedItems)
          {
            _optionalRemoveCallback?.Invoke(item);
          }
        }

        // Return the count of affected items
        return (TResult)(object)affectedItems.Count;
      }
      // Fall back to default expression execution
      return base.Execute<TResult>(expression);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
      return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    private static void ApplyUpdateChangesToDbSet(IEnumerable<T> affectedItems, MethodCallExpression methodCallExpression)
    {
      // Extract the update lambda: opt => opt.SetProperty(...)
      if ((methodCallExpression.Arguments[1] as UnaryExpression)?.Operand is not LambdaExpression updateLambda)
      {
        return;
      }

      // The body may be a chain of MethodCallExpressions
      var currentCall = updateLambda.Body as MethodCallExpression;
      while (currentCall != null && currentCall.Method.Name == "SetProperty")
      {
        var memberExpr = currentCall.Arguments[0] as LambdaExpression;
        var valueExpr = currentCall.Arguments[1];

        var propertyInfo = ((memberExpr.Body as MemberExpression)?.Member) as System.Reflection.PropertyInfo;
        var value = Expression.Lambda(valueExpr).Compile().DynamicInvoke();

        foreach (var item in affectedItems)
        {
          propertyInfo?.SetValue(item, value);
        }

        // Move to the next chained call (if any)
        currentCall = currentCall.Object as MethodCallExpression;
      }
    }
  }
}
