using System.Diagnostics;

namespace Assertivo.Exceptions;

/// <summary>
/// Assertions for an <see cref="IAsyncEnumerable{T}"/> subject.
/// Obtain an instance via <c>source.Should()</c>.
/// </summary>
public readonly struct AsyncEnumerableAssertions<T>
{
    internal AsyncEnumerableAssertions(IAsyncEnumerable<T>? subject, string? expression)
    {
        Subject = subject;
        Expression = expression;
    }

    /// <summary>Gets the async enumerable under test.</summary>
    public IAsyncEnumerable<T>? Subject { get; }

    internal string? Expression { get; }

    /// <summary>
    /// Asserts that the async enumerable throws an exception of type
    /// <typeparamref name="TException"/> or a subtype when enumerated.
    /// Unwraps <see cref="AggregateException"/> with a single inner exception.
    /// </summary>
    /// <typeparam name="TException">The expected exception type (exact or base).</typeparam>
    /// <param name="because">An optional reason phrase for the assertion.</param>
    /// <param name="becauseArgs">Optional format arguments for <paramref name="because"/>.</param>
    /// <returns>
    /// A task resolving to <see cref="ExceptionAssertions{TException}"/> for
    /// inspecting the caught exception via <c>.Which</c>.
    /// </returns>
    [StackTraceHidden]
    public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(
        string because = "", params object[] becauseArgs)
        where TException : Exception
    {
        if (Subject is null)
            MessageFormatter.Fail("source to be non-null", "source was null", Expression, because, becauseArgs);

        Exception? caught = null;
        var enumerator = Subject!.GetAsyncEnumerator();
        try
        {
            while (true)
            {
                bool hasNext;
                try { hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false); }
                catch (Exception ex) { caught = ex; break; }
                if (!hasNext) break;
            }
        }
        finally
        {
            try { await enumerator.DisposeAsync().ConfigureAwait(false); }
            catch when (caught is not null) { /* discard — original iteration exception takes precedence */ }
        }

        if (caught is not null)
        {
            var target = caught;
            if (caught is AggregateException { InnerExceptions.Count: 1 } ae)
                target = ae.InnerExceptions[0];

            if (target is TException typed)
                return new ExceptionAssertions<TException>(typed, Expression);

            MessageFormatter.Fail(
                typeof(TException).Name,
                $"{target.GetType().Name}: {target.Message}",
                Expression,
                because,
                becauseArgs);
            return default!;
        }

        MessageFormatter.Fail(
            $"source to throw {typeof(TException).Name}",
            "no exception was thrown",
            Expression,
            because,
            becauseArgs);
        return default!;
    }
}
