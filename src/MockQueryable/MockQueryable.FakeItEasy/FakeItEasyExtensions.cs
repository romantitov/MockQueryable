using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
			return mock;
		}

		[Obsolete("Use BuildMockDbSet<TEntity> instead")]
		public static DbQuery<TEntity> BuildMockDbQuery<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
			var mock = A.Fake<DbQuery<TEntity>>(
				d => d.Implements<IAsyncEnumerable<TEntity>>().Implements<IQueryable<TEntity>>());
			var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data);
			mock.ConfigureQueryableCalls(enumerable, data);
			mock.ConfigureAsyncEnumerableCalls(enumerable);
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
	}
}