using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using MockQueryable.Core;

namespace MockQueryable.EntityFrameworkCore
{
  public class TestAsyncEnumerableEfCore<T, TExpressionVisitor>: TestQueryProvider<T, TExpressionVisitor>, IAsyncEnumerable<T>, IAsyncQueryProvider
    where TExpressionVisitor : ExpressionVisitor, new()
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
        .Invoke(this, [expression]);

      return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
        .MakeGenericMethod(expectedResultType)
        .Invoke(null, [executionResult]);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
      return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }
  }
}
