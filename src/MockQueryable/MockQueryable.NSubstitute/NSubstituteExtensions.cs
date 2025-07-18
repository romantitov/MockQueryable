using Microsoft.EntityFrameworkCore;
using MockQueryable.Core;
using MockQueryable.EntityFrameworkCore;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MockQueryable.NSubstitute
{
  public static class NSubstituteExtensions
  {
    public static DbSet<TEntity> BuildMockDbSet<TEntity, TExpressionVisitor>(this IEnumerable<TEntity> data)
      where TEntity : class
      where TExpressionVisitor : ExpressionVisitor, new()
      => data.BuildMock<TEntity, TExpressionVisitor>().BuildMockDbSet();

    public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IEnumerable<TEntity> data) where TEntity : class => data.BuildMock().BuildMockDbSet();

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
    public static DbSet<TEntity> BuildMockDbSet<TEntity>(this IQueryable<TEntity> data) where TEntity : class
    {
      return BuildMockDbSet<TEntity, TestExpressionVisitor>(data);
    }

    /// <summary>
    /// See <see cref="BuildMockDbSet{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of the entity that the DbSet will represent.
    /// </typeparam>
    /// <typeparam name="TExpressionVisitor">
    /// The type of the expression visitor that will be used to process LINQ expressions.
    /// Can be used to mock EF Core specific expression handling, such as for ILike expressions.
    /// </typeparam>
    public static DbSet<TEntity> BuildMockDbSet<TEntity, TExpressionVisitor>(this IQueryable<TEntity> data)
      where TEntity : class
      where TExpressionVisitor : ExpressionVisitor, new()
    {
      var mock = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();
      var enumerable = new TestAsyncEnumerableEfCore<TEntity, TExpressionVisitor>(data);

      mock.ConfigureAsyncEnumerableCalls(enumerable);
      mock.ConfigureQueryableCalls(enumerable, data);
      mock.ConfigureDbSetCalls(data);

      if (mock is IAsyncEnumerable<TEntity> asyncEnumerable)
      {
        asyncEnumerable.GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(args => enumerable.GetAsyncEnumerator());
      }

      return mock;
    }

    private static void ConfigureQueryableCalls<TEntity>(
      this IQueryable<TEntity> mock,
      IQueryProvider queryProvider,
      IQueryable<TEntity> data) where TEntity : class
    {
      mock.Provider.Returns(queryProvider);
      mock.Expression.Returns(data?.Expression);
      mock.ElementType.Returns(data?.ElementType);
      mock.GetEnumerator().Returns(info => data?.GetEnumerator());
    }

    private static void ConfigureAsyncEnumerableCalls<TEntity>(
      this DbSet<TEntity> mock,
      IAsyncEnumerable<TEntity> enumerable) where TEntity : class
    {
      mock.GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(args => enumerable.GetAsyncEnumerator());
    }

    private static void ConfigureDbSetCalls<TEntity>(this DbSet<TEntity> mock, IQueryable<TEntity> data)
      where TEntity : class
    {
      mock.AsQueryable().Returns(data);
      mock.AsAsyncEnumerable().Returns(args => CreateAsyncMock(data));
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