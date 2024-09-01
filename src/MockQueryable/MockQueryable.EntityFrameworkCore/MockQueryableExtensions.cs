using MockQueryable.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

// Moving MockQueryableExtensions BuildMock into the MockQueryable.EntityFrameworkCore
// namespace had breaking changes with earlier extensions added to MockQueryable.Moq
// and other previous extension method locations. Moving this extension up a namespace to
// MockQueryable aleviates that breaking change. It still needs to remain in EF core since it
// is dependent on the EF Core AsyncEnumerable.
namespace MockQueryable
{
    public static class MockQueryableExtensions
    {
        public static IQueryable<TEntity> BuildMock<TEntity>(this IEnumerable<TEntity> data) where TEntity : class
        {
            return new TestAsyncEnumerableEfCore<TEntity>(data);
        }
    }
}
