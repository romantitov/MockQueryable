using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace MockQueryable
{
	public class TestAsyncEnumerable<T> : IAsyncEnumerable<T>, IOrderedQueryable<T>, IAsyncQueryProvider
	{
		private IEnumerable<T> _enumerable;

		public TestAsyncEnumerable(Expression expression)
		{
			Expression = expression;
		}

		public TestAsyncEnumerable(IEnumerable<T> enumerable)
		{
			_enumerable = enumerable;
		}

		public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
		}

		public IQueryable CreateQuery(Expression expression)
		{
			if (expression is MethodCallExpression m)
			{
				var resultType = m.Method.ReturnType; // it should be IQueryable<T>
				var tElement = resultType.GetGenericArguments().First();
				var queryType = typeof(TestAsyncEnumerable<>).MakeGenericType(tElement);
				return (IQueryable) Activator.CreateInstance(queryType, expression);
			}

			return new TestAsyncEnumerable<T>(expression);
		}

		public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
		{
			return new TestAsyncEnumerable<TEntity>(expression);
		}

		public object Execute(Expression expression)
		{
			return CompileExpressionItem<object>(expression);
		}

		public TResult Execute<TResult>(Expression expression)
		{
			return CompileExpressionItem<TResult>(expression);
		}

		public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
		{
			var expectedResultType = typeof(TResult).GetGenericArguments()[0];
			var executionResult = typeof(IQueryProvider)
        .GetMethods()
        .First(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
        .MakeGenericMethod(expectedResultType)
				.Invoke(this, new[] {expression});

			return (TResult) typeof(Task).GetMethod(nameof(Task.FromResult))
				.MakeGenericMethod(expectedResultType)
				.Invoke(null, new[] {executionResult});
		}


		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			if (_enumerable == null) _enumerable = CompileExpressionItem<IEnumerable<T>>(Expression);
			return _enumerable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (_enumerable == null) _enumerable = CompileExpressionItem<IEnumerable<T>>(Expression);
			return _enumerable.GetEnumerator();
		}

		public Type ElementType => typeof(T);

		public Expression Expression { get; }

		public IQueryProvider Provider => this;

		private static TResult CompileExpressionItem<TResult>(Expression expression)
		{
			var rewriter = new TestExpressionVisitor();
			var body = rewriter.Visit(expression);
			var f = Expression.Lambda<Func<TResult>>(body, (IEnumerable<ParameterExpression>) null);
			return f.Compile()();
		}
	}
}