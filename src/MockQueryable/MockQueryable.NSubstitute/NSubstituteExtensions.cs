using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace MockQueryable.NSubstitute
{
    public static class NSubstituteExtensions
    {
        public static IQueryable<TEntity> BuildMock<TEntity>(this IQueryable<TEntity> data) where TEntity : class
        {
            var mock = Substitute.For<IQueryable<TEntity>, IAsyncEnumerable<TEntity>> ();
            var enumerable = new TestAsyncEnumerable<TEntity>(data);
            ((IAsyncEnumerable<TEntity>)mock).GetEnumerator().Returns(enumerable.GetEnumerator());
            mock.Provider.Returns(enumerable);
            mock.Expression.Returns(data?.Expression);
            mock.ElementType.Returns(data?.ElementType);
            mock.GetEnumerator().Returns(data?.GetEnumerator());
            return mock;
        }

        public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
        {
            var mock = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();
            var enumerable = new TestAsyncEnumerable<TEntity>(data);
            ((IAsyncEnumerable<TEntity>)mock).GetEnumerator().Returns(enumerable.GetEnumerator());
            var queryable = ((IQueryable<TEntity>)mock);
            queryable.Provider.Returns(enumerable);
            queryable.Expression.Returns(data?.Expression);
            queryable.ElementType.Returns(data?.ElementType);
            queryable.GetEnumerator().Returns(data?.GetEnumerator());
            return mock;
        }

        public static DbQuery<TEntity> BuildMockDbQuery<TEntity>(this IQueryable<TEntity> data) where TEntity : class
        {
            var mock = Substitute.For<DbQuery<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();
            var enumerable = new TestAsyncEnumerable<TEntity>(data);
            ((IAsyncEnumerable<TEntity>)mock).GetEnumerator().Returns(enumerable.GetEnumerator());
            var queryable = ((IQueryable<TEntity>)mock);
            queryable.Provider.Returns(enumerable);
            queryable.Expression.Returns(data?.Expression);
            queryable.ElementType.Returns(data?.ElementType);
            queryable.GetEnumerator().Returns(data?.GetEnumerator());
            return mock;
        }
    }
}
