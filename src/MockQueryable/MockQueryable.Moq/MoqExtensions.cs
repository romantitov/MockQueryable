using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using MockQueryable.EntityFrameworkCore;
using Moq;

namespace MockQueryable.Moq
{
	public static class MoqExtensions
	{
		public static Mock<IQueryable<TEntity>> BuildMock<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
			var mock = new Mock<IQueryable<TEntity>>();
			var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data);
			mock.As<IAsyncEnumerable<TEntity>>().ConfigureAsyncEnumerableCalls(enumerable);
			mock.ConfigureQueryableCalls(enumerable, data);
			return mock;
		}

		public static Mock<DbSet<TEntity>> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
			var mock = new Mock<DbSet<TEntity>>();
			var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data);
			mock.As<IAsyncEnumerable<TEntity>>().ConfigureAsyncEnumerableCalls(enumerable);
			mock.As<IQueryable<TEntity>>().ConfigureQueryableCalls(enumerable, data);
			mock.ConfigureDbSetCalls();
			return mock;
		}

		[Obsolete("Use BuildMockDbSet<TEntity> instead")]
		public static Mock<DbQuery<TEntity>> BuildMockDbQuery<TEntity>(this IQueryable<TEntity> data)
			where TEntity : class
		{
			var mock = new Mock<DbQuery<TEntity>>();
			var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data);
			mock.As<IAsyncEnumerable<TEntity>>().ConfigureAsyncEnumerableCalls(enumerable);
			mock.As<IQueryable<TEntity>>().ConfigureQueryableCalls(enumerable, data);
			return mock;
		}

		private static void ConfigureDbSetCalls<TEntity>(this Mock<DbSet<TEntity>> mock) 
			where TEntity : class
		{
			mock.Setup(m => m.AsQueryable()).Returns(mock.Object);
			mock.Setup(m => m.AsAsyncEnumerable()).Returns(mock.Object);
		}

		private static void ConfigureQueryableCalls<TEntity>(
			this Mock<IQueryable<TEntity>> mock,
			IQueryProvider queryProvider,
			IQueryable<TEntity> data) where TEntity : class
		{
			mock.Setup(m => m.Provider).Returns(queryProvider);
			mock.Setup(m => m.Expression).Returns(data?.Expression);
			mock.Setup(m => m.ElementType).Returns(data?.ElementType);
			mock.Setup(m => m.GetEnumerator()).Returns(() => data?.GetEnumerator());
		}

		private static void ConfigureAsyncEnumerableCalls<TEntity>(
			this Mock<IAsyncEnumerable<TEntity>> mock,
			IAsyncEnumerable<TEntity> enumerable)
		{
			mock.Setup(d => d.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
				.Returns(() => enumerable.GetAsyncEnumerator());
		}
	}
}