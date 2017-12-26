using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MockQueryable
{
	public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
	{
		private readonly IEnumerator<T> _enumerator;

		public TestAsyncEnumerator(IEnumerator<T> enumerator)
		{
			_enumerator = enumerator ?? throw new ArgumentNullException();
		}

		public T Current => _enumerator.Current;

		public void Dispose()
		{
		}

		public Task<bool> MoveNext(CancellationToken cancellationToken)
		{
			return Task.FromResult(_enumerator.MoveNext());
		}
	}
}