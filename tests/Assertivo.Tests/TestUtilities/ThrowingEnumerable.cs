using System.Collections;

namespace Assertivo.Tests.TestUtilities;

internal sealed class ThrowingEnumerable<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _source;
    private readonly int _throwAfterMoveNextCount;
    private readonly Exception _exception;

    internal ThrowingEnumerable(IEnumerable<T> source, int throwAfterMoveNextCount, Exception? exception = null)
    {
        _source = source;
        _throwAfterMoveNextCount = throwAfterMoveNextCount;
        _exception = exception ?? new InvalidOperationException("Source enumeration failed.");
    }

    public IEnumerator<T> GetEnumerator() => new ThrowingEnumerator(_source.GetEnumerator(), _throwAfterMoveNextCount, _exception);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class ThrowingEnumerator : IEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        private readonly int _throwAfterMoveNextCount;
        private readonly Exception _exception;
        private int _moveNextCount;

        internal ThrowingEnumerator(IEnumerator<T> inner, int throwAfterMoveNextCount, Exception exception)
        {
            _inner = inner;
            _throwAfterMoveNextCount = throwAfterMoveNextCount;
            _exception = exception;
        }

        public T Current => _inner.Current;

        object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            _moveNextCount++;
            if (_moveNextCount > _throwAfterMoveNextCount)
            {
                throw _exception;
            }

            return _inner.MoveNext();
        }

        public void Reset() => _inner.Reset();

        public void Dispose() => _inner.Dispose();
    }
}
