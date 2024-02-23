using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MockQueryable.EntityFrameworkCore;
using Moq;

namespace MockQueryable.Moq
{
	public static class MoqExtensions
	{
		public static IQueryable<TEntity> BuildMock<TEntity>(this IEnumerable<TEntity> data) where TEntity : class
		{
            return new TestAsyncEnumerableEfCore<TEntity>(data);
		}

		public static Mock<DbSet<TEntity>> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
			var mock = new Mock<DbSet<TEntity>>();
			var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data);
			mock.ConfigureAsyncEnumerableCalls(enumerable);
			mock.As<IQueryable<TEntity>>().ConfigureQueryableCalls(enumerable, data);
            mock.As<IAsyncEnumerable<TEntity>>().Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(() => enumerable.GetAsyncEnumerator());
			mock.Setup(m => m.AsQueryable()).Returns(enumerable);

			mock.ConfigureDbSetCalls(data);
			return mock;
		}

		private static void ConfigureDbSetCalls<TEntity>(this Mock<DbSet<TEntity>> mock, IQueryable<TEntity> data) 
			where TEntity : class
		{
			mock.Setup(m => m.AsQueryable()).Returns(mock.Object);
			mock.Setup(m => m.AsAsyncEnumerable()).Returns(CreateAsyncMock(data));
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
			this Mock<DbSet<TEntity>> mock,
			IAsyncEnumerable<TEntity> enumerable)where TEntity : class
		{
			mock.Setup(d => d.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
				.Returns(() => enumerable.GetAsyncEnumerator());
            
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