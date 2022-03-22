using System.Collections.Generic;
using System.Linq;

namespace MockQueryable.EntityFrameworkCore
{
    public static class EnumerableExtensions
    {
        public static IQueryable<T> AsTestAsyncQueryable<T>(this IEnumerable<T> query)
        {
            return new TestAsyncEnumerableEfCore<T>(query);
        }
    }
}