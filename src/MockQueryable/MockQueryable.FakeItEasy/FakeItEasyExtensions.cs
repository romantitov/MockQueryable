using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;

namespace MockQueryable.FakeItEasy
{
    public static class FakeItEasyExtensions
    {
        public static IQueryable<TEntity> BuildMock<TEntity>(this IQueryable<TEntity> data) where TEntity : class
        {
            var mock = A.Fake<IQueryable<TEntity>>(d=>d.Implements<IAsyncEnumerable<TEntity>>().Implements<IQueryable<TEntity>>());
            var enumerable = new TestAsyncEnumerable<TEntity>(data);
            A.CallTo(() => ((IAsyncEnumerable<TEntity>) mock).GetEnumerator()).Returns(enumerable.GetEnumerator());
            A.CallTo(() => ((IQueryable<TEntity>)mock).Provider).Returns(enumerable);
            A.CallTo(() => ((IQueryable<TEntity>)mock).Expression).Returns(data?.Expression);
            A.CallTo(() => ((IQueryable<TEntity>)mock).ElementType).Returns(data?.ElementType);
            A.CallTo(() => ((IQueryable<TEntity>)mock).GetEnumerator()).Returns(data?.GetEnumerator());
            return mock;
        }

        public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
        {
            var mock = A.Fake<DbSet<TEntity>>(d => d.Implements<IAsyncEnumerable<TEntity>>().Implements<IQueryable<TEntity>>());
            var enumerable = new TestAsyncEnumerable<TEntity>(data);
            A.CallTo(() => ((IAsyncEnumerable<TEntity>)mock).GetEnumerator()).Returns(enumerable.GetEnumerator());
            A.CallTo(() => ((IQueryable<TEntity>)mock).Provider).Returns(enumerable);
            A.CallTo(() => ((IQueryable<TEntity>)mock).Expression).Returns(data?.Expression);
            A.CallTo(() => ((IQueryable<TEntity>)mock).ElementType).Returns(data?.ElementType);
            A.CallTo(() => ((IQueryable<TEntity>)mock).GetEnumerator()).Returns(data?.GetEnumerator());
            return mock;
        }

        public static DbQuery<TEntity> BuildMockDbQuery<TEntity>(this IQueryable<TEntity> data) where TEntity : class
        {
            var mock = A.Fake<DbQuery<TEntity>>(d => d.Implements<IAsyncEnumerable<TEntity>>().Implements<IQueryable<TEntity>>());
            var enumerable = new TestAsyncEnumerable<TEntity>(data);
            A.CallTo(() => ((IAsyncEnumerable<TEntity>)mock).GetEnumerator()).Returns(enumerable.GetEnumerator());
            A.CallTo(() => ((IQueryable<TEntity>)mock).Provider).Returns(enumerable);
            A.CallTo(() => ((IQueryable<TEntity>)mock).Expression).Returns(data?.Expression);
            A.CallTo(() => ((IQueryable<TEntity>)mock).ElementType).Returns(data?.ElementType);
            A.CallTo(() => ((IQueryable<TEntity>)mock).GetEnumerator()).Returns(data?.GetEnumerator());
            return mock;
        }
    }
}
