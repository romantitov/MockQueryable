using Microsoft.EntityFrameworkCore;
using MockQueryable.Core;
using MockQueryable.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MockQueryable.Moq
{
	public static class MoqExtensions
	{
        
		public static Mock<DbSet<TEntity>> BuildMockDbSet<TEntity, TExpressionVisitor>(this IEnumerable<TEntity> data)
			where TEntity : class
      where TExpressionVisitor : ExpressionVisitor, new()
      => data.BuildMock<TEntity, TExpressionVisitor>().BuildMockDbSet();

    public static Mock<DbSet<TEntity>> BuildMockDbSet<TEntity>(this IEnumerable<TEntity> data)
      where TEntity : class
      => data.BuildMock().BuildMockDbSet();

    /// <summary>
    /// This method allows you to create a mock DbSet for testing purposes.
    /// It is particularly useful when you want to simulate the behavior of Entity Framework Core's DbSet
    /// with custom expression handling, such as for testing LINQ queries or database operations.
    /// The method takes an IQueryable of the entity type and returns a mocked DbSet that implements
    /// both IAsyncEnumerable and IQueryable interfaces, allowing for asynchronous enumeration
    /// and LINQ query capabilities.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of the entity that the DbSet will represent.
    /// </typeparam>
    public static Mock<DbSet<TEntity>> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
      return BuildMockDbSet<TEntity, TestExpressionVisitor>(data);
    }

    /// <summary>
    /// This method allows you to create a mock DbSet for testing purposes.
    /// It is particularly useful when you want to simulate the behavior of Entity Framework Core's DbSet
    /// with custom expression handling, such as for testing LINQ queries or database operations.
    /// The method takes an IQueryable of the entity type and returns a mocked DbSet that implements
    /// both IAsyncEnumerable and IQueryable interfaces, allowing for asynchronous enumeration
    /// and LINQ query capabilities.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of the entity that the DbSet will represent.
    /// </typeparam>
    /// <typeparam name="TExpressionVisitor">
    /// The type of the expression visitor that will be used to process LINQ expressions.
    /// Can be used to mock EF Core specific expression handling, such as for ILike expressions.
    /// </typeparam>
    public static Mock<DbSet<TEntity>> BuildMockDbSet<TEntity, TExpressionVisitor>(this IQueryable<TEntity> data)
			where TEntity : class
      where TExpressionVisitor : ExpressionVisitor, new()
    {
      var mock = new Mock<DbSet<TEntity>>();
      var enumerable = new TestAsyncEnumerableEfCore<TEntity, TExpressionVisitor>(data);
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