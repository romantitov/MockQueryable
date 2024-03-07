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
        public static IQueryable<TEntity> BuildMock<TEntity>(this IEnumerable<TEntity> data) where TEntity : class
        {
            return new TestAsyncEnumerableEfCore<TEntity>(data);
        }

        public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
        {
            var mock = A.Fake<DbSet<TEntity>>(d => d.Implements<IAsyncEnumerable<TEntity>>().Implements<IQueryable<TEntity>>());
            var enumerable = new TestAsyncEnumerableEfCore<TEntity>(data);
            mock.ConfigureQueryableCalls(enumerable, data);
            mock.ConfigureAsyncEnumerableCalls(enumerable);
            mock.ConfigureDbSetCalls(data);
            if (mock is IAsyncEnumerable<TEntity> asyncEnumerable)
            {
                A.CallTo(() => asyncEnumerable.GetAsyncEnumerator(A<CancellationToken>.Ignored)).ReturnsLazily(() => enumerable.GetAsyncEnumerator());
            }
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
            this DbSet<TEntity> mock,
            IAsyncEnumerable<TEntity> enumerable) where TEntity : class
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