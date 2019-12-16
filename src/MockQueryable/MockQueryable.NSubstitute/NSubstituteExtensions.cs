using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace MockQueryable.NSubstitute
{
	public static class NSubstituteExtensions
	{
		public static IQueryable<TEntity> BuildMock<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
			var mock = Substitute.For<IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();
			var enumerable = new TestAsyncEnumerable<TEntity>(data);
			((IAsyncEnumerable<TEntity>) mock).ConfigureAsyncEnumerableCalls(enumerable);
			mock.ConfigureQueryableCalls(enumerable, data);
			return mock;
		}

		public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
			var mock = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();
			var enumerable = new TestAsyncEnumerable<TEntity>(data);
			mock.ConfigureAsyncEnumerableCalls(enumerable);
			mock.ConfigureQueryableCalls(enumerable, data);
			return mock;
		}

		[Obsolete("Use BuildMockDbSet<TEntity> instead")]
		public static DbQuery<TEntity> BuildMockDbQuery<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
			var mock = Substitute.For<DbQuery<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();
			var enumerable = new TestAsyncEnumerable<TEntity>(data);
			mock.ConfigureAsyncEnumerableCalls(enumerable);
			mock.ConfigureQueryableCalls(enumerable, data);
			return mock;
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
			this IAsyncEnumerable<TEntity> mock,
			IAsyncEnumerable<TEntity> enumerable)
		{
			mock.GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(args => enumerable.GetAsyncEnumerator());
		}
	}
}