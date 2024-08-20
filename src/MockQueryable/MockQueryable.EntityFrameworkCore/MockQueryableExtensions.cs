using System.Collections.Generic;
using System.Linq;


namespace MockQueryable.EntityFrameworkCore
{
    public static class MockQueryableExtensions
    {
        public static IQueryable<TEntity> BuildMock<TEntity>(this IEnumerable<TEntity> data) where TEntity : class
		{
            return new TestAsyncEnumerableEfCore<TEntity>(data);
		}
    }
}
