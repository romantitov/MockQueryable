using System.Collections.Generic;
using System.Linq;
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
            mock.Expression.Returns(data.Expression);
            mock.ElementType.Returns(data.ElementType);
            mock.GetEnumerator().Returns(data.GetEnumerator());
            return mock;
        }
    }
}
