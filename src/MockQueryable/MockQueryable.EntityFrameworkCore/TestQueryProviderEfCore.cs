using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MockQueryable.Core;

namespace MockQueryable.EntityFrameworkCore
{
  public class TestAsyncEnumerableEfCore<T>: TestQueryProvider<T>, IAsyncEnumerable<T>, IAsyncQueryProvider
  {
    public TestAsyncEnumerableEfCore(Expression expression) : base(expression)
    {
    }

    public TestAsyncEnumerableEfCore(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
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
        if (expression is MethodCallExpression methodCall && (methodCall.Method.Name == nameof(RelationalQueryableExtensions.ExecuteUpdate) || methodCall.Method.Name == nameof(RelationalQueryableExtensions.ExecuteDelete) )
                                                          && typeof(TResult) == typeof(int))
        {
            // Intercept ExecuteDelete and ExecuteUpdate calls
            var affectedItems = base.Execute<IEnumerable<T>>(Expression).ToList();
         
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
  }
}
