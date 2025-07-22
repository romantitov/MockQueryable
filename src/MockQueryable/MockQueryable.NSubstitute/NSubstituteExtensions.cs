using Microsoft.EntityFrameworkCore;
using MockQueryable.EntityFrameworkCore;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MockQueryable.NSubstitute
{
  public static class NSubstituteExtensions
  {
    public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IEnumerable<TEntity> data) where TEntity : class => data.BuildMock().BuildMockDbSet();

    public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
    {
      var mock = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();
      var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data);

      ConfigureAllCalls(mock, enumerable, data);

      return mock;
    }

    public static DbSet<TEntity> BuildMockDbSetWithInterceptedExecuteDelete<TEntity>(
      this ICollection<TEntity> data) where TEntity : class
    {
      var mock = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();
      var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data, x => data.Remove(x));

      ConfigureAllCalls(mock, enumerable, data.AsQueryable());

      return mock;
    }

    public static DbSet<TEntity> BuildMockDbSetWithInterceptedExecuteDelete<TEntity>(
        this DbSet<TEntity> data) where TEntity : class
    {
      var mock = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();
      var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data, x => data.Remove(x));

      ConfigureAllCalls(mock, enumerable, data.AsQueryable());

      return mock;
    }

    private static void ConfigureAllCalls<TEntity>(
        this DbSet<TEntity> mock,
        TestAsyncEnumerableEfCore<TEntity> enumerable,
        IQueryable<TEntity> data) where TEntity : class
    {
      mock.ConfigureAsyncEnumerableCalls(enumerable);
      mock.ConfigureQueryableCalls(enumerable, data);
      mock.ConfigureDbSetCalls(data);

      if (mock is IAsyncEnumerable<TEntity> asyncEnumerable)
      {
        asyncEnumerable.GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(args => enumerable.GetAsyncEnumerator());
      }
    }

    private static void ConfigureQueryableCalls<TEntity>(
      this IQueryable<TEntity> mock,
      IQueryProvider queryProvider,
      IQueryable<TEntity> data) where TEntity : class
    {
      mock.Provider.Returns(queryProvider);
      mock.Expression.Returns(data?.Expression);
      mock.ElementType.Returns(data?.ElementType);
      mock.GetEnumerator().Returns(info => data?.GetEnumerator());
    }

    private static void ConfigureAsyncEnumerableCalls<TEntity>(
      this DbSet<TEntity> mock,
      IAsyncEnumerable<TEntity> enumerable) where TEntity : class
    {
      mock.GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(args => enumerable.GetAsyncEnumerator());
    }

    private static void ConfigureDbSetCalls<TEntity>(this DbSet<TEntity> mock, IQueryable<TEntity> data)
      where TEntity : class
    {
      mock.AsQueryable().Returns(data);
      mock.AsAsyncEnumerable().Returns(args => CreateAsyncMock(data));
    }

    private static async IAsyncEnumerable<TEntity> CreateAsyncMock<TEntity>(IEnumerable<TEntity> data)
      where TEntity : class
    {
      foreach (var entity in data)
      {
        yield return entity;
      }

      await Task.CompletedTask;
    }
  }
}