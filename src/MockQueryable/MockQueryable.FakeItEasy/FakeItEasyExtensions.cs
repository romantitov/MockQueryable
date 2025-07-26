using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using MockQueryable.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MockQueryable.FakeItEasy
{
  public static class FakeItEasyExtensions
  {
    public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IEnumerable<TEntity> data) where TEntity : class => data.BuildMock().BuildMockDbSet();

    public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
    {
      var mock = A.Fake<DbSet<TEntity>>(d => d.Implements<IAsyncEnumerable<TEntity>>().Implements<IQueryable<TEntity>>());
      var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data);

      ConfigureAllCalls(mock, enumerable, data);

      return mock;
    }

    public static DbSet<TEntity> BuildMockDbSetWithInterceptedExecuteDelete<TEntity>(
      this ICollection<TEntity> data) where TEntity : class
    {
      var mock = A.Fake<DbSet<TEntity>>(d => d.Implements<IAsyncEnumerable<TEntity>>().Implements<IQueryable<TEntity>>());
      var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data, x => data.Remove(x));

      ConfigureAllCalls(mock, enumerable, data.AsQueryable());

      return mock;
    }

    public static DbSet<TEntity> BuildMockDbSetWithInterceptedExecuteDelete<TEntity>(
        this DbSet<TEntity> data) where TEntity : class
    {
      var mock = A.Fake<DbSet<TEntity>>(d => d.Implements<IAsyncEnumerable<TEntity>>().Implements<IQueryable<TEntity>>());
      var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data, x => data.Remove(x));

      ConfigureAllCalls(mock, enumerable, data);

      return mock;
    }

    private static void ConfigureAllCalls<TEntity>(
        this DbSet<TEntity> mock,
        TestAsyncEnumerableEfCore<TEntity> enumerable,
        IQueryable<TEntity> data) where TEntity : class
    {
      mock.ConfigureQueryableCalls(enumerable, data);
      mock.ConfigureAsyncEnumerableCalls(enumerable);
      mock.ConfigureDbSetCalls(data);
      if (mock is IAsyncEnumerable<TEntity> asyncEnumerable)
      {
        A.CallTo(() => asyncEnumerable.GetAsyncEnumerator(A<CancellationToken>.Ignored)).ReturnsLazily(() => enumerable.GetAsyncEnumerator());
      }
    }

    private static void ConfigureQueryableCalls<TEntity>(
        this IQueryable<TEntity> mock,
        IQueryProvider queryProvider,
        IQueryable<TEntity> data) where TEntity : class
    {
      A.CallTo(() => mock.Provider).Returns(queryProvider);
      A.CallTo(() => mock.Expression).Returns(data?.Expression);
      A.CallTo(() => mock.ElementType).Returns(data?.ElementType);
      A.CallTo(() => mock.GetEnumerator()).ReturnsLazily(() => data?.GetEnumerator());
    }

    private static void ConfigureAsyncEnumerableCalls<TEntity>(
        this DbSet<TEntity> mock,
        IAsyncEnumerable<TEntity> enumerable) where TEntity : class
    {
      A.CallTo(() => mock.GetAsyncEnumerator(A<CancellationToken>.Ignored))
          .Returns(enumerable.GetAsyncEnumerator());

    }

    private static void ConfigureDbSetCalls<TEntity>(this DbSet<TEntity> mock, IQueryable<TEntity> data)
      where TEntity : class
    {
      A.CallTo(() => mock.AsQueryable()).Returns(data.AsQueryable());
      A.CallTo(() => mock.AsAsyncEnumerable()).ReturnsLazily(args => CreateAsyncMock(mock));
    }

    private static async IAsyncEnumerable<TEntity> CreateAsyncMock<TEntity>(
      this IQueryable<TEntity> mock)
      where TEntity : class
    {
      var data = await mock.ToListAsync();

      foreach (var entity in data)
      {
        yield return entity;
      }

      await Task.CompletedTask;
    }
  }
}