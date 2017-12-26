using System.Collections.Generic;
using System.Linq;
using Moq;

namespace MockQueryable.Moq
{
	public static class MoqExtensions
	{
		public static Mock<IQueryable<TEntity>> BuildMock<TEntity>(this IQueryable<TEntity> data) where TEntity : class
		{
			var mock = new Mock<IQueryable<TEntity>>();
			var enumerable = new TestAsyncEnumerable<TEntity>(data);
			mock.As<IAsyncEnumerable<TEntity>>().Setup(d => d.GetEnumerator()).Returns(enumerable.GetEnumerator);
			mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(enumerable);
			mock.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(data.Expression);
			mock.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
			mock.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
			return mock;
		}
	}
}
