using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using MockQueryable.EntityFrameworkCore;

namespace MockQueryable.FakeItEasy
{
	public static class FakeItEasyExtensions
	{
		public static IQueryable<TEntity> BuildMock<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
			var mock = A.Fake<IQueryable<TEntity>>(
				d => d.Implements<IAsyncEnumerable<TEntity>>().Implements<IQueryable<TEntity>>());
			var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data);
			((IAsyncEnumerable<TEntity>) mock).ConfigureAsyncEnumerableCalls(enumerable);
			mock.ConfigureQueryableCalls(enumerable, data);

			return mock;
		}

		public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
			var mock = A.Fake<DbSet<TEntity>>(
				d => d.Implements<IAsyncEnumerable<TEntity>>().Implements<IQueryable<TEntity>>());
			var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data);
			mock.ConfigureQueryableCalls(enumerable, data);
			mock.ConfigureAsyncEnumerableCalls(enumerable);
      mock.ConfigureDbSetCalls(data);
			return mock;
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
			this IAsyncEnumerable<TEntity> mock,
			IAsyncEnumerable<TEntity> enumerable)
		{
			A.CallTo(() => mock.GetAsyncEnumerator(A<CancellationToken>.Ignored))
				.Returns(enumerable.GetAsyncEnumerator());
		}

    private static void ConfigureDbSetCalls<TEntity>(this DbSet<TEntity> mock, IQueryable<TEntity> data)
      where TEntity : class
    {
			A.CallTo(() => mock.AsQueryable()).Returns(data);
			A.CallTo(() => mock.AsAsyncEnumerable()).ReturnsLazily(args => CreateAsyncMock(data));
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